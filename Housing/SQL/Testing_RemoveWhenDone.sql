USE ICS_NET_TEST
BEGIN TRANSACTION
	
ROLLBACK

--select * from FWK_User where email = 'kpeterson7@carthage.edu'


BEGIN TRANSACTION
	/*
	INSERT INTO
		CUS_Housing_RoomStudent (RoomStudentID, StudentID, RoomSessionID, OaksWaitlist)
	SELECT
		NEWID(), 'CF8CB16E-BC7E-4A6F-8060-F0D1C2FDB980', HRS.RoomSessionID, 'N'
	FROM
		CUS_Housing_Room	HR	INNER JOIN	CUS_Housing_RoomSession	HRS		ON	HR.RoomID			=	HRS.RoomID
								LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
								INNER JOIN	CUS_Housing_Building	HB		ON	HR.BuildingID		=	HB.BuildingID
	WHERE
		HRS.RoomSessionID	=	'9CE13A4F-9041-4633-9754-4AE83755EA48'
	AND
		--Is the gender of the room either blank (unassigned) or a match to the student's gender
		HRS.Gender			IN	('', 'F')
	GROUP BY
		HRS.RoomSessionID, HR.Capacity
	--Is there still at least one available bed in the room?
	HAVING
		COUNT(HRStu.RoomStudentID)	<	HR.Capacity;

	*/

	SELECT * FROM CUS_Housing_RoomReservation;
	EXECUTE CUS_spHousingRegisterRoom @uuidStudentid = 'CF8CB16E-BC7E-4A6F-8060-F0D1C2FDB980', @uuidRoomSessionID = '06EA22DD-4262-411D-BE9A-D478986FE6CA', @strGender = 'F', @strOaksWaitlist = 'N';
	SELECT * FROM CUS_Housing_RoomStudent;
	SELECT * FROM CUS_Housing_RoomReservation;
ROLLBACK


SELECT * FROM CUS_Housing_Room WHERE BuildingID = (SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'DEN') AND RoomNumber = '016'
SELECT * FROM CUS_Housing_RoomSession WHERE RoomID = '9CE13A4F-9041-4633-9754-4AE83755EA48' AND HousingYear = YEAR(GETDATE())

SELECT
	HB.BuildingName, HR.RoomNumber, HR.Capacity, ISNULL(FU.FirstName,'') AS FirstName, ISNULL(FU.LastName,'') AS LastName
FROM
	CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR		ON	HRS.RoomID			=	HR.RoomID
								INNER JOIN	CUS_Housing_Building	HB		ON	HR.BuildingID		=	HB.BuildingID
								LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
								LEFT JOIN	FWK_User				FU		ON	HRStu.StudentID		=	FU.ID
WHERE
	HRS.RoomSessionID	=	'418981EC-6921-48E3-939A-5B74C00D4AA3'

SELECT
	FU.FirstName, FU.LastName, FU.Email
FROM
	FWK_User	FU	LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	FU.ID				=	HRStu.StudentID
					LEFT JOIN	CUS_Housing_RoomSession	HRS		ON	HRStu.RoomSessionID	=	HRS.RoomSessionID
																AND	HRS.HousingYear		=	YEAR(GETDATE())
WHERE
	FU.HostID = 371861


UPDATE CUS_HousingSelectionStartdate SET startdate = '2015-04-27' WHERE id = 1
UPDATE CUS_HousingSelectionStartdate SET startdate = '2015-04-08' WHERE id = 2

SELECT * FROM CUS_HousingSelectionStartdate


BEGIN TRANSACTION
	--DECLARE @uuidStudentID	UNIQUEIDENTIFIER	=	'8909f201-26dd-4c9b-b58c-b7fae239391d';
	--DECLARE	@uuidInviterID	UNIQUEIDENTIFIER	=	'CF8CB16E-BC7E-4A6F-8060-F0D1C2FDB980';
	DECLARE @uuidStudentID	UNIQUEIDENTIFIER	=	'CF8CB16E-BC7E-4A6F-8060-F0D1C2FDB980';
	DECLARE	@uuidInviterID	UNIQUEIDENTIFIER	=	'8909f201-26dd-4c9b-b58c-b7fae239391d';

	INSERT INTO CUS_Housing_RoomReservation (RoomSessionID, StudentID, ReservationTime, CreatedByID)
	SELECT
		HRS.RoomSessionID, @uuidStudentID, GETDATE(), @uuidInviterID
	FROM
		CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID		=	HR.RoomID
																			AND	HRS.HousingYear	=	YEAR(GETDATE())
									INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
	WHERE
		HB.BuildingCode	=		'OAK1'
		AND
		HR.RoomNumber	LIKE	'50%'

	SELECT * FROM CUS_Housing_RoomReservation ORDER BY ReservationTime
ROLLBACK

DECLARE @uuidStudentID	UNIQUEIDENTIFIER	=	'8909f201-26dd-4c9b-b58c-b7fae239391d';
SELECT
	HB.BuildingName, HR.RoomNumber, HRR.RoomSessionID,
	CASE
		WHEN	FU.ID	IS	NULL	THEN	''
									ELSE	FU.FirstName + ' ' + FU.LastName
	END	AS	InvitedBy
FROM
	CUS_Housing_RoomReservation	HRR	INNER JOIN	CUS_Housing_RoomSession	HRS	ON	HRR.RoomSessionID	=	HRS.RoomSessionID
																			AND	HRS.HousingYear		=	YEAR(GETDATE())
									INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID			=	HR.RoomID
									INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID		=	HB.BuildingID
									LEFT JOIN	FWK_User				FU	ON	HRR.CreatedByID		=	FU.ID
WHERE
	HRR.StudentID	=	@uuidStudentID
    AND
    HRS.Gender      IN  ('', 'F')
ORDER BY
	HB.BuildingID, HR.RoomNumber

USE ICS_NET;
BEGIN TRANSACTION
	--SELECT * FROM FWK_ConfigSettings WHERE Category = 'C_Session'
	--SELECT * FROM FWK_User WHERE LastName = 'Summers'
	EXEC dbo.CUS_spHousingRegisterRoom @uuidStudentID = 'ABD95812-8F32-4C5C-86CC-3545C1D768C4', @uuidRoomSessionID = '6ECA1EA8-33A4-48D0-BA98-A6B6F4BD5229', @strGender = 'M'
ROLLBACK

BEGIN TRANSACTION
	--EXEC dbo.CUS_spHousingRegisterRoom @uuidStudentID = '8909f201-26dd-4c9b-b58c-b7fae239391d', @uuidRoomSessionID = 'd0b99a8e-bdfd-48df-a48c-3cf3a12d02e3', @strGender = 'F'
	
	SELECT * FROM CUS_Housing_RoomStudent;
	
	SELECT HRS.RoomSessionID, HRS.Gender, HB.BuildingCode, HR.RoomNumber
	FROM CUS_Housing_RoomSession HRS INNER JOIN CUS_Housing_Room HR ON HRS.RoomID = HR.RoomID INNER JOIN CUS_Housing_Building HB ON HR.BuildingID = HB.BuildingID
	WHERE HB.BuildingCode = 'OAK1' AND HRS.HousingYear = YEAR(GETDATE())
	ORDER BY HB.BuildingCode, HR.RoomNumber;

	UPDATE CUS_Housing_RoomSession SET Gender = '' WHERE RoomSessionID IN ('A62311FA-AA2F-4E66-9C3B-AA0C03C7FFA4','451FA22B-F440-4770-AB1F-6D847175C2DB');

	UPDATE
		HRS
	SET
		Gender	=	''
	FROM
		CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID		=	HR.RoomID
																			AND	HRS.HousingYear	=	YEAR(GETDATE())
									INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
	WHERE
		HB.BuildingCode	LIKE	'OAK%'
		

	DELETE
		HRStu
	FROM
		CUS_Housing_RoomStudent	HRStu	INNER JOIN	CUS_Housing_RoomSession	HRS	ON	HRStu.RoomSessionID	=	HRS.RoomSessionID
	WHERE
		HRS.HousingYear	=	YEAR(GETDATE());
ROLLBACK