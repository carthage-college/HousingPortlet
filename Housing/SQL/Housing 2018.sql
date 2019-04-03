USE ICS_NET;
BEGIN TRANSACTION
	

	--DECLARE	@buildingID	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TOWR'),
	--		@oaksID		UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'OAK4');
	
	--EXECUTE CUS_spHousing_getRoomsStandard @guidBuildingID = @buildingID, @strGender = 'M';

	--EXECUTE CUS_spHousing_getRoomsOaks @guidBuildingID = @oaksID, @strGender = 'M';
ROLLBACK

--BEGIN TRANSACTION
--	DECLARE	@studentID	UNIQUEIDENTIFIER	=	(SELECT ID FROM FWK_User WHERE LastName = 'Armitage' And FirstName = 'Amanda'),
--			@roomID		UNIQUEIDENTIFIER	=	'0696BD7D-1A4A-4971-A6B9-CB9649FEBBB8'; --TOWR 206B

--	SELECT
--		HRS.*, HR.RoomNumber
--	FROM
--		CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room	HR	ON	HRS.RoomID		=	HR.RoomID
--																		AND	HRS.HousingYear	=	YEAR(GETDATE())
--	WHERE
--		HRS.RoomID IN (SELECT RoomID FROM CUS_Housing_Room WHERE BuildingID = (SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TOWR'))
--	ORDER BY
--		HR.RoomNumber

--	EXECUTE CUS_spHousingRegisterRoom @uuidStudentID = @studentID, @uuidRoomSessionID = @roomID, @strGender = 'F', @oaksWaitlist = 'N';

--	SELECT
--		HRS.*, HR.RoomNumber
--	FROM
--		CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room	HR	ON	HRS.RoomID		=	HR.RoomID
--																		AND	HRS.HousingYear	=	YEAR(GETDATE())
--	WHERE
--		HRS.RoomID IN (SELECT RoomID FROM CUS_Housing_Room WHERE BuildingID = (SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TOWR'))
--	ORDER BY
--		HR.RoomNumber

--	EXECUTE CUS_spHousing_getRoomDetails @guidRoomSessionID = @roomID;
--ROLLBACK

--BEGIN TRANSACTION
	--UPDATE CUS_Housing_RoomSession
	--SET		CUS_Housing_RoomSession.GreekID	=	CUS_Housing_Room.DefaultGreek
	--FROM	CUS_Housing_RoomSession	INNER JOIN	CUS_Housing_Room ON CUS_Housing_RoomSession.RoomID = CUS_Housing_Room.RoomID AND CUS_Housing_RoomSession.HousingYear = YEAR(GETDATE())
--ROLLBACK

--DECLARE @studentID	UNIQUEIDENTIFIER	=	(SELECT ID FROM FWK_User WHERE LastName = 'Armitage' And FirstName = 'Amanda'),
--		@buildingID	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'JOH');
--EXECUTE CUS_spHousing_getRoomsGreekSquatter @strGreekID = 'S007', @strGender = 'F', @guidStudentID = @studentID, @guidBuildingID = @buildingID;

--EXECUTE CUS_spHousing_getStartDates

--SELECT
--	HRS.*, HR.RoomNumber
--FROM
--	CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room	HR	ON	HRS.RoomID		=	HR.RoomID
--																	AND	HRS.HousingYear	=	YEAR(GETDATE())
--WHERE
--	HRS.RoomID IN (SELECT RoomID FROM CUS_Housing_Room WHERE BuildingID = (SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TOWR'))
--ORDER BY
--	HR.RoomNumber
        
--SELECT * FROM CUS_HousingSelectionGreek
--SELECT * FROM CUS_Housing_Room WHERE BuildingID = (SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'JOH') ORDER BY RoomNumber
--UPDATE CUS_HousingSelectionStartdate SET startdate = '2018-03-23' WHERE id = 1;
--UPDATE CUS_HousingSelectionStartdate SET startdate = '2018-03-24' WHERE id = 2;
--SELECT * FROM CUS_HousingSelectionStartdate WHERE id = 2

BEGIN TRANSACTION
	DECLARE	@strConfigCategory				VARCHAR(50)	=	'C_Housing',
			@strKeyEndTime					VARCHAR(50)	=	'END_TIME',
			@strValueEndTime				VARCHAR(50)	=	15,
			@strKeyIsProduction				VARCHAR(50)	=	'IS_PRODUCTION',
			@strValueIsProduction			VARCHAR(50)	=	'True',
			@strKeySendEmail				VARCHAR(50)	=	'SEND_EMAIL',
			@strValueSendEmail				VARCHAR(50)	=	'True',
			@strKeyTestEmail				VARCHAR(50)	=	'TEST_EMAIL',
			@strValueTestEmail				VARCHAR(50)	=	'mkishline@carthage.edu',
			@strKeyHousingDirectorName		VARCHAR(50)	=	'HOUSING_DIRECTOR_NAME',
			@strValueHousingDirectorName	VARCHAR(50)	=	'Amber Krusza',
			@strKeyHousingDirectorEmail		VARCHAR(50)	=	'HOUSING_DIRECTOR_EMAIL',
			@strValueHousingDirectorEmail	VARCHAR(50)	=	'akrusza@carthage.edu';

	IF NOT EXISTS (SELECT ID FROM FWK_ConfigSettings WHERE Category = @strConfigCategory AND [Key] = @strKeyEndTime)
		BEGIN
			INSERT INTO FWK_ConfigSettings(ID, Category, [Key], [Value], DefaultValue)
			VALUES (NEWID(), @strConfigCategory, @strKeyEndTime, @strValueEndTime, @strValueEndTime)
		END

	IF NOT EXISTS (SELECT ID FROM FWK_ConfigSettings WHERE Category = @strConfigCategory AND [Key] = @strKeyIsProduction)
		BEGIN
			INSERT INTO FWK_ConfigSettings(ID, Category, [Key], [Value], DefaultValue)
			VALUES (NEWID(), @strConfigCategory, @strKeyIsProduction, @strValueIsProduction, @strValueIsProduction)
		END
	ELSE
		BEGIN
			UPDATE FWK_ConfigSettings SET [Value] = @strValueIsProduction, DefaultValue = @strValueIsProduction WHERE Category = @strConfigCategory AND [Key] = @strKeyIsProduction
		END

	IF NOT EXISTS (SELECT ID FROM FWK_ConfigSettings WHERE Category = @strConfigCategory AND [Key] = @strKeySendEmail)
		BEGIN
			INSERT INTO FWK_ConfigSettings(ID, Category, [Key], [Value], DefaultValue)
			VALUES (NEWID(), @strConfigCategory, @strKeySendEmail, @strValueSendEmail, @strValueSendEmail)
		END
	ELSE
		BEGIN
			UPDATE FWK_ConfigSettings SET [Value] = @strValueSendEmail, DefaultValue = @strValueSendEmail WHERE Category = @strConfigCategory AND [Key] = @strKeySendEmail
		END

	IF NOT EXISTS (SELECT ID FROM FWK_ConfigSettings WHERE Category = @strConfigCategory AND [Key] = @strKeyTestEmail)
		BEGIN
			INSERT INTO FWK_ConfigSettings(ID, Category, [Key], [Value], DefaultValue)
			VALUES (NEWID(), @strConfigCategory, @strKeyTestEmail, @strValueTestEmail, @strValueTestEmail)
		END
	ELSE
		BEGIN
			UPDATE FWK_ConfigSettings SET [Value] = @strValueTestEmail, DefaultValue = @strValueTestEmail WHERE Category = @strConfigCategory AND [Key] = @strKeyTestEmail
		END

	IF NOT EXISTS (SELECT ID FROM FWK_ConfigSettings WHERE Category = @strConfigCategory AND [Key] = @strKeyHousingDirectorName)
		BEGIN
			INSERT INTO FWK_ConfigSettings(ID, Category, [Key], [Value], DefaultValue)
			VALUES (NEWID(), @strConfigCategory, @strKeyHousingDirectorName, @strValueHousingDirectorName, @strValueHousingDirectorName)
		END

	IF NOT EXISTS (SELECT ID FROM FWK_ConfigSettings WHERE Category = @strConfigCategory AND [Key] = @strKeyHousingDirectorEmail)
		BEGIN
			INSERT INTO FWK_ConfigSettings(ID, Category, [Key], [Value], DefaultValue)
			VALUES (NEWID(), @strConfigCategory, @strKeyHousingDirectorEmail, @strValueHousingDirectorEmail, @strValueHousingDirectorEmail)
		END


	SELECT * FROM FWK_ConfigSettings WHERE Category = 'C_Housing';


	IF OBJECT_ID('dbo.CUS_spHousing_getHousingSetting', 'P') IS NOT NULL
		DROP PROCEDURE dbo.CUS_spHousing_getHousingSetting;
	GO
	CREATE PROCEDURE [dbo].[CUS_spHousing_getHousingSetting]
		@strSettingKey	NVARCHAR(255),
		@strCategory	VARCHAR(50)	=	'C_Housing'
	AS
	BEGIN
		SELECT
			[Value] AS SettingValue
		FROM
			FWK_ConfigSettings
        WHERE
			Category	=	@strCategory
		AND
			[Key]		=	@strSettingKey
	END

ROLLBACK