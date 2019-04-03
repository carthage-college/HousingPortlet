SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Mike Kishline
-- Create date: 3/1/2019
-- Last update:	3/1/2019
-- Description:	Retrieves available rooms from the Residential Tower
-- =============================================

--If this stored procedure already exists in the database, remove and recreate it
IF OBJECT_ID('dbo.CUS_spHousing_getRoomsTower', 'P') IS NOT NULL
    DROP PROCEDURE dbo.CUS_spHousing_getRoomsTower;
GO

CREATE PROCEDURE [dbo].[CUS_spHousing_getRoomsTower]
	--The gender of the student
	@strGender	CHAR(1)	=	'',
	--Only return RA rooms
	@bitIsRA	BIT		=	NULL,
	--Housing sign-up year. Other than testing, this should always be the current calendar year
	@intYear	INT		=	YEAR(GETDATE())
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON;

	--The building ID of the Residential Tower
	DECLARE	@uuidTowerID		UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TOWR');

	SELECT
		HB.BuildingName, HB.BuildingCode, SuiteA.RoomNumberOnly, SuiteA.RoomSessionID, SuiteA.Capacity + ISNULL(SuiteB.Capacity, 0) AS RoomCapacity,
		SuiteA.Capacity AS SuiteA_Capacity, SuiteB.RoomSessionID, ISNULL(SuiteB.Capacity, 0) AS SuiteB_Capacity
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
		HB.BuildingName, SuiteA.RoomNumberOnly, Suiteb.RoomNumberOnly
END
GO