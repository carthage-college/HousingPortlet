USE ICS_NET_TEST;
--DELETE FROM CUS_Housing_RoomReservation
--DELETE FROM CUS_Housing_RoomStudent

--SELECT * FROM FWK_User WHERE LastName = 'Arient'
--DECLARE @uuidBuilding UNIQUEIDENTIFIER = (SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TAR')
--EXECUTE CUS_spHousing_getRoomsOaks @guidBuildingID = @uuidOaks, @strGender = 'F'
--EXECUTE CUS_spHousing_getRoomsStandard @guidBuildingID = @uuidBuilding, @strGender = 'F'

BEGIN TRANSACTION
	---------------------------------------------------------------------------------
	--	Creation of Housing View
	---------------------------------------------------------------------------------
	IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[CUS_VW_Housing_Main]'))
		EXEC sp_executesql N'CREATE VIEW [dbo].[CUS_VW_Housing_Main] AS SELECT ''This stub will be replaced by an Alter statement'' as [code_stub]';
	GO

	ALTER VIEW [dbo].[CUS_VW_Housing_Main]
	AS
		SELECT
			--Building Information
			HB.BuildingID, HB.BuildingCode, HB.BuildingName,
			--Room Information
			HR.RoomNumber, LEFT(HR.RoomNumber, 3) AS RoomNumberOnly, HR.Capacity,
			--RoomSession Information
			HRS.RoomSessionID, HRS.Gender, Other.RoomSessionID AS AdjoiningRoomSessionID,
			--Occupants
			COUNT(HRStu.RoomSessionID) AS Occupants, COUNT(HRR.RoomSessionID) AS Invitations, COUNT(HRSTU.RoomSessionID) + COUNT(HRR.RoomSessionID) AS Total
		FROM
			CUS_Housing_Building	HB	INNER JOIN	CUS_Housing_Room			HR		ON	HB.BuildingID		=	HR.BuildingID
										INNER JOIN	CUS_Housing_RoomSession		HRS		ON	HR.RoomID			=	HRS.RoomID
										LEFT JOIN	CUS_Housing_RoomStudent		HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
										LEFT JOIN	CUS_Housing_RoomReservation	HRR		ON	HRS.RoomSessionID	=	HRR.RoomSessionID
										LEFT JOIN
										(
											SELECT
												RoomSessionID, LEFT(HR.RoomNumber, 3) AS RoomNumberOnly, HB.BuildingCode
											FROM
												CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID		=	HR.RoomID
																			INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
										)										Other	ON	HB.BuildingCode		=	Other.BuildingCode
																						AND	LEFT(HR.RoomNumber, 3)	=	Other.RoomNumberOnly
																						AND	HRS.RoomSessionID	<>	Other.RoomSessionID
		GROUP BY
			HB.BuildingID, HB.BuildingCode, HB.BuildingName, HR.RoomNumber, LEFT(HR.RoomNumber, 3), HR.Capacity, HRS.RoomSessionID, Other.RoomSessionID, HRS.Gender
	GO


	---------------------------------------------------------------------------------
	--	Update invitation room detail stored procedure
	---------------------------------------------------------------------------------
	IF OBJECT_ID('dbo.CUS_spHousing_getInvitationRoomDetails', 'P') IS NOT NULL
		DROP PROCEDURE [dbo].[CUS_spHousing_getInvitationRoomDetails];
	GO
	CREATE PROCEDURE [dbo].[CUS_spHousing_getInvitationRoomDetails]
		@guidRoomSessionID	UNIQUEIDENTIFIER
	AS
	BEGIN
		SELECT
			BuildingName, BuildingCode, RoomNumberOnly, SUM(Capacity) AS Capacity, SUM(Occupants + Invitations) AS OccupantAndInvited
		FROM
			CUS_VW_Housing_Main
		WHERE
			@guidRoomSessionID	IN	(RoomSessionID, AdjoiningRoomSessionID)
		GROUP BY
			BuildingName, BuildingCode, RoomNumberOnly
	END
	GO

	---------------------------------------------------------------------------------
	--	Update reservation stored procedure
	---------------------------------------------------------------------------------
	IF OBJECT_ID('dbo.CUS_spHousing_insertReservation', 'P') IS NOT NULL
		DROP PROCEDURE [dbo].[CUS_spHousing_insertReservation];
	GO
	CREATE PROCEDURE [dbo].[CUS_spHousing_insertReservation]
		@guidRoomSession	UNIQUEIDENTIFIER,
		@guidInvitee		UNIQUEIDENTIFIER,
		@guidInviter		UNIQUEIDENTIFIER
	AS
	BEGIN
		DECLARE	@guidNewRoomSessionID	UNIQUEIDENTIFIER;
		SELECT
			--EXACT.*, ADJACENT.BuildingID AS AltBuilding, ADJACENT.RoomNumberOnly AS AltRoom, ADJACENT.RoomSessionID AS AltSess,
			@guidNewRoomSessionID = CASE WHEN EXACT.Capacity > EXACT.Occupants THEN EXACT.RoomSessionID ELSE ADJACENT.RoomSessionID END --AS RoomSessionInvited
		FROM
			CUS_Housing_Building	HB	INNER JOIN
			(
				SELECT
					HR.BuildingID, HRS.RoomSessionID, LEFT(HR.RoomNumber, 3) AS RoomNumberOnly, HR.Capacity, COUNT(HRStu.RoomStudentID) AS Occupants
				FROM
					CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR		ON	HRS.RoomID			=	HR.RoomID
												LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
																							AND	HRS.HousingYear		=	YEAR(GETDATE())
				WHERE
					HRS.RoomSessionID	=	@guidRoomSession
				GROUP BY
					HR.BuildingID, HR.Capacity, HRS.RoomSessionID, LEFT(HR.RoomNumber, 3)
			)	EXACT	ON	HB.BuildingID	=	EXACT.BuildingID
			LEFT JOIN
			(
				SELECT
					HR.BuildingID, HRS.RoomSessionID, LEFT(HR.RoomNumber, 3) AS RoomNumberOnly
				FROM
					CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR		ON	HRS.RoomID			=	HR.RoomID
												LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
				WHERE
					HRS.RoomSessionID	<>	@guidRoomSession
			)	ADJACENT	ON	EXACT.RoomNumberOnly	=	ADJACENT.RoomNumberOnly
							AND	EXACT.BuildingID		=	ADJACENT.BuildingID

        INSERT INTO CUS_Housing_RoomReservation (RoomSessionID, StudentID, ReservationTime, CreatedByID)
        VALUES(@guidNewRoomSessionID, @guidInvitee, GETDATE(), @guidInviter)

		SELECT @guidNewRoomSessionID AS 'RoomSessionID';
	END
	GO


	IF OBJECT_ID('dbo.CUS_spHousing_getRoomsTower', 'P') IS NOT NULL
		DROP PROCEDURE [dbo].[CUS_spHousing_getRoomsTower];
	GO
	CREATE PROCEDURE [dbo].[CUS_spHousing_getRoomsTower]
		--The gender of the student
		@strGender	CHAR(1)	=	'',
		--Only return RA rooms
		@bitIsRA	BIT		=	NULL,
		--Housing sign-up year. Other than testing, this should always be the current calendar year
		@intYear	INT		=	NULL
	AS
	BEGIN
		--The building ID of the Residential Tower
		DECLARE	@uuidTowerID		UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TOWR');

		--Housing sign-up year. Other than testing, this should always be the current calendar year
		SET		@intYear	=	ISNULL(@intYear, YEAR(GETDATE()));

		SELECT
			HB.BuildingName, HB.BuildingCode, SuiteA.RoomNumberOnly, SuiteA.RoomSessionID AS RoomID1, SuiteA.Capacity + ISNULL(SuiteB.Capacity, 0) AS RoomCapacity,
			SuiteA.Capacity AS SuiteA_Capacity, SuiteB.RoomSessionID AS RoomID2, ISNULL(SuiteB.Capacity, 0) AS SuiteB_Capacity
		FROM
			CUS_Housing_Building	HB	LEFT JOIN	(
														SELECT
															HR_A.BuildingID, HRS_A.RoomSessionID, LEFT(HR_A.RoomNumber, 3) AS RoomNumberOnly, HR_A.Capacity, HRS_A.Gender, HR_A.IsRA
														FROM
															CUS_Housing_Room	HR_A	INNER JOIN	CUS_Housing_RoomSession	HRS_A	ON	HR_A.RoomID			=	HRS_A.RoomID
																																	AND	HRS_A.HousingYear	=	@intYear
														WHERE
															HR_A.BuildingID	=	@uuidTowerID
														AND
															RIGHT(HR_A.RoomNumber, 1)	=	'A'
													)	SuiteA	ON	HB.BuildingID	=	SuiteA.BuildingID
										LEFT JOIN	(
														SELECT
															HR_B.BuildingID, HRS_B.RoomSessionID, LEFT(HR_B.RoomNumber, 3) AS RoomNumberOnly, HR_B.Capacity, HRS_B.Gender, HR_B.IsRA
														FROM
															CUS_Housing_Room	HR_B	INNER JOIN	CUS_Housing_RoomSession	HRS_B	ON	HR_B.RoomID			=	HRS_B.RoomID
																																	AND	HRS_B.HousingYear	=	@intYear
														WHERE
															HR_B.BuildingID	=	@uuidTowerID
														AND
															RIGHT(HR_B.RoomNumber, 1)	=	'B'
													)	SuiteB	ON	HB.BuildingID			=	SuiteB.BuildingID
																AND	SuiteA.RoomNumberOnly	=	SuiteB.RoomNumberOnly
		WHERE
			HB.BuildingID				=	@uuidTowerID
		AND
			ISNULL(SuiteA.Gender, '')	IN	(@strGender, '')
		AND
			(
				@bitIsRA				IS	NULL
				OR
				@bitIsRA				IN	(SuiteA.IsRA, SuiteB.IsRA)
			)
		ORDER BY
			HB.BuildingName, SuiteA.RoomNumberOnly
	END
	GO

	/*
	---------------------------------------------------------------------------------
	--	Begin testing
	---------------------------------------------------------------------------------
	DECLARE @guidVanessaAcosta		UNIQUEIDENTIFIER	=	'155AA03B-1975-46A6-B5D8-8FE84813A896',
			@guidAdriannaAcosta		UNIQUEIDENTIFIER	=	'4A53ED90-B428-4163-ACDE-954570BD7D15',
			@guidNicoleActon		UNIQUEIDENTIFIER	=	'4A119EE1-6098-41A1-A605-E98A06E4172C',
			@guidAfiaAfrifa			UNIQUEIDENTIFIER	=	'188F71F9-AF37-49B2-B847-28EECB0DF8BE',
			@guidAmandaArmitage		UNIQUEIDENTIFIER	=	'1AF1F9D9-1E67-4946-BED7-D645A7E01A67',
			@guidKaitlinArnashus	UNIQUEIDENTIFIER	=	'D55BB991-2B0A-4449-A21A-73AA50FBE0CB',
			@guidErinApplegate		UNIQUEIDENTIFIER	=	'A05ECE1C-1ADC-4A6F-93DC-CAB5D51650A0',
			@guidDaphneAdamson		UNIQUEIDENTIFIER	=	'57C553F9-6B98-4D04-ACDB-1C07751C7DFA',
			@guidCarolineAlbers		UNIQUEIDENTIFIER	=	'66D2B27B-3C19-4DDF-9D54-A6316D393745',
			@guidAlexisLein			UNIQUEIDENTIFIER	=	'0EEA49B4-D812-4FF5-9FD4-C49F30471865',
			@guidNatalieNuzzo		UNIQUEIDENTIFIER	=	'FD5AB0A5-2435-4993-9613-C65F099D5192',
			@guidMelanieMcClure		UNIQUEIDENTIFIER	=	'F25BBD08-EFF5-4AE9-8AB5-C9EB07DE9755',
			
			@guidMichaelArient		UNIQUEIDENTIFIER	=	'F992E5CD-72CA-4E04-B945-79AD66EDD9EB';

	DECLARE	@uidTarble				UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TAR'),
			--Rooms
			@uidTower205A			UNIQUEIDENTIFIER	=	'330ACAB5-F23B-41F0-9CCA-D099B1EFCC47',
			@uidTower205B			UNIQUEIDENTIFIER	=	'3E669502-3789-4656-A4C3-44F504D9F196',
			@uidTarble006			UNIQUEIDENTIFIER	=	'32992BC0-2C4D-4C51-AD85-C4E7F4A24B6B';

	DECLARE	@strBuildingCode	VARCHAR(5)	=	'TOWR',
			@strRoomNumber		VARCHAR(5)	=	'205';

	EXECUTE [dbo].[CUS_spHousing_getInvitationRoommates] @strBuildingCode = @strBuildingCode, @strRoomNumber = @strRoomNumber;

	EXECUTE CUS_spHousingRegisterRoom @uuidStudentID = @guidVanessaAcosta, @uuidRoomSessionID = @uidTower205B, @strGender = 'F';
	EXECUTE CUS_spHousing_insertReservation @guidRoomSession = @uidTower205B, @guidInvitee = @guidAdriannaAcosta, @guidInviter = @guidVanessaAcosta;
	EXECUTE CUS_spHousing_insertReservation @guidRoomSession = @uidTower205A, @guidInvitee = @guidNicoleActon, @guidInviter = @guidVanessaAcosta;
	--EXECUTE CUS_spHousing_insertReservation @guidRoomSession = @uidSuiteB, @guidInvitee = @guidAfiaAfrifa, @guidInviter = @guidVanessaAcosta;

	EXECUTE CUS_spHousing_getInvitationRoommates @strBuildingCode = @strBuildingCode, @strRoomNumber = @strRoomNumber;
	
	SELECT * FROM CUS_VW_Housing_Main WHERE RoomSessionID = @uidTower205B;

	SELECT * FROM CUS_VW_Housing_Main WHERE @uidTower205A IN (RoomSessionID, AdjoiningRoomSessionID);
	EXECUTE CUS_spHousing_getInvitationRoomDetails @guidRoomSessionID = @uidTower205A;
	


	SET	@strBuildingCode	=	'TAR';
	SET	@strRoomNumber		=	'006';
	EXECUTE [dbo].[CUS_spHousing_getInvitationRoommates] @strBuildingCode = @strBuildingCode, @strRoomNumber = @strRoomNumber;
	EXECUTE CUS_spHousingRegisterRoom @uuidStudentID = @guidAmandaArmitage, @uuidRoomSessionID = @uidTarble006, @strGender = 'F';
	EXECUTE CUS_spHousing_insertReservation @guidRoomSession = @uidTarble006, @guidInvitee = @guidKaitlinArnashus, @guidInviter = @guidAmandaArmitage;
	EXECUTE [dbo].[CUS_spHousing_getInvitationRoommates] @strBuildingCode = @strBuildingCode, @strRoomNumber = @strRoomNumber;
	
	SELECT * FROM CUS_VW_Housing_Main WHERE @uidTarble006 IN (RoomSessionID, AdjoiningRoomSessionID);
	*/
ROLLBACK