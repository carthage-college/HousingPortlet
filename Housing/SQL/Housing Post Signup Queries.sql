USE ICS_NET
SELECT PPL.*, FU.FirstName, FU.LastName FROM CUS_ProfilePageLayout PPL INNER JOIN FWK_User FU ON PPL.UserID = FU.ID ORDER BY LastName, FirstName, ColumnIndex, Sequence


BEGIN TRANSACTION
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('0791DB87-735C-4947-9593-08437834E876', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('34A38B1B-A521-4F4F-BC9F-1973539ABC3D', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('86B0B6BD-5239-4B6A-85CC-24471A8F9001', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('BF4A8F86-53BC-4AD2-B5CC-66FEC936A85F', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('3177A296-0F50-4850-B126-6833515F7128', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('377F4916-C0E1-4FDD-922C-71736902572D', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('39C05CC6-7A4A-44A9-8AA2-A9C8F0D1E999', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('9F129645-E28A-4CD5-92D4-B050C8DB7D72', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('F69AB229-EB31-4DC7-827D-CC113CED29E7', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('7B2E5A1E-63BF-43B9-A33C-D08C91BA8E31', 12, 3, 11, 1)
	INSERT INTO CUS_ProfilePageLayout (UserID, BoxID, ColumnIndex, Sequence, DefaultExpanded) VALUES ('2D6602F8-1439-4763-8E26-D10B0D752816', 12, 3, 11, 1)
ROLLBACK











SELECT * FROM FWK_User WHERE HostID = 371861

BEGIN TRANSACTION
	SELECT COUNT(*) FROM CUS_HousingAssignmentSurvey WHERE [matched] = 'N'
	UPDATE CUS_HousingAssignmentSurvey SET [matched] = '2015RA' WHERE [matched] = 'N'
	SELECT COUNT(*) FROM CUS_HousingAssignmentSurvey WHERE [matched] = 'N'
	SELECT COUNT(*) FROM CUS_HousingAssignmentSurvey WHERE [matched] = '2015RA'
ROLLBACK

SELECT * FROM FWK_User WHERE lastname = 'Lazar' and FirstName = 'Kelly'
SELECT * FROM CUS_ProfilePageLayout WHERE UserID = '377F4916-C0E1-4FDD-922C-71736902572D' ORDER BY BoxID
--DELETE FROM CUS_ProfilePageLayout WHERE PageLayoutID IN (19414,19421,19430,19404,19416,19423,19432,19425,19418,19407,19426,19420,19435,19411,19427,19419,19406,19433,19424,19413,19428,19429,19431,19410)

SELECT * FROM FWK_LoggedEmail ORDER BY FWK_LoggedEmail.SendTime DESC

SELECT * FROM FWK_ConfigSettings  ORDER BY [Key] WHERE FWK_ConfigSettings.[Key] = 'IA_CRM_MAX_GIFTS'

SELECT * FROM FWK_User WHERE LastName = 'Robinson' And FirstName = 'David'

BEGIN TRANSACTION
ROLLBACK

SELECT * FROM FWK_User WHERE LastName = 'Kishline'

/********	Housing Management Queries - Post Signup	*********/

/* Find records of paired students to be assigned to a room */
	SELECT
		CUS_HousingAssignments.id, 
		student1.fname as fname1, student1.lname as lname1, student1.new_id as surveyid1, student1.id as stu_id1,
		student2.fname as fname2, student2.lname as lname2, student2.new_id as surveyid2, student2.id as stu_id2,
		CUS_HousingAssignments.earliestdate
	FROM
		CUS_HousingAssignments	INNER JOIN	CUS_HousingAssignmentSurvey	student1	ON	CUS_HousingAssignments.id1	=	student1.new_id
								INNER JOIN	CUS_HousingAssignmentSurvey	student2	ON	CUS_HousingAssignments.id2	=	student2.new_id
	WHERE
		CUS_HousingAssignments.status = 'F'
	ORDER BY
		earliestdate

/* Using an ID from the above query, get the specific record and flag the matched pair as assigned to a room */
	SELECT * FROM CUS_HousingAssignments WHERE id = 2115
	UPDATE CUS_HousingAssignments SET [status] = 'P' WHERE id = 2115

/*
	Process to assign a student (generally a freshman) to a room
	1) Get the student's FWK_User information
	2) Put the student's HostID into @intCXID and their ID into uuidStudent
	3) Set the building and room (no letter for Oaks suites)
	4) Execute the two select statements which get the student's fall room assignment (if present) and the occupants of the room
	5) If the fall assignment is empty and there is still space in the room, copy the RoomSessionID to uuidRoom
	6) Make sure the gender is correct and execute the stored procedure
*/
--Get the student's FWK_User information
SELECT * FROM FWK_User WHERE LastName = 'Schwerdtfeger' and FirstName = 'Robert'
BEGIN TRANSACTION
	DECLARE	@intCXID		INT				=	00001315014,
			@strBldg		VARCHAR(255)	=	'SWE',
			@strRoom		VARCHAR(255)	=	'015'

	--Get student's upcoming session room assignment
	SELECT
		HB.BuildingCode, HR.RoomNumber
	FROM
		CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID		=	HR.RoomID
																			AND	HRS.HousingYear	=	YEAR(GETDATE())
									INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
									INNER JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
									INNER JOIN	FWK_User				FU	ON	HRStu.StudentID	=	FU.ID
	WHERE
		FU.HostID	=	@intCXID

	--Get occupants of room for upcoming session
	SELECT
		FU.HostID, FU.FirstName, FU.LastName, HB.BuildingCode, HR.RoomNumber, HR.Capacity, HRS.RoomSessionID, HRS.Gender
	FROM
		CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID		=	HR.RoomID
																			AND	HRS.HousingYear	=	YEAR(GETDATE())
									INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
									LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
									LEFT JOIN	FWK_User				FU	ON	HRStu.StudentID	=	FU.ID
	WHERE
		HB.BuildingCode	=	@strBldg
	AND
		HR.RoomNumber	LIKE	@strRoom + '%'
	ORDER BY
		HR.RoomNumber

	--UPDATE CUS_Housing_RoomSession SET Gender = '' WHERE RoomSessionID = '39B44546-FBCB-43C4-A307-A30D36DE3BD5'
	UPDATE CUS_Housing_RoomSession SET Gender = '' WHERE RoomSessionID = '0FC032E7-FAEB-4233-83A4-2E25F8439546'

	--Assign student to room
	DECLARE	@uuidStudent	VARCHAR(255)	=	'6E27ADCC-AFF3-4EA6-A7DB-099738967838'
			,@uuidRoom		VARCHAR(255)	=	'0FC032E7-FAEB-4233-83A4-2E25F8439546'
			,@strGender		VARCHAR(1)		=	'M'
    EXECUTE CUS_spHousingRegisterRoom
        @uuidStudentID      =   @uuidStudent,
        @uuidRoomSessionID  =   @uuidRoom,
        @strGender          =   @strGender
ROLLBACK
Cristin


--Create room reservation
BEGIN TRANSACTION
	DECLARE @lastname	VARCHAR(50)	=	'Good'
	DECLARE	@firstname	VARCHAR(50)	=	'Justice'
	SELECT
		FU.*
	FROM
		FWK_User	FU
	WHERE
		LastName	=	@lastname
	AND
		FirstName	=	@firstname
	ORDER BY
		FirstName


	DECLARE	@createdBy	UNIQUEIDENTIFIER	=	(SELECT ID FROM FWK_User WHERE Email = 'jgaudio@carthage.edu')
	DECLARE @invitee	UNIQUEIDENTIFIER	=	'E52685BF-A8D1-475F-877D-8C4308E41509'
	INSERT INTO CUS_Housing_RoomReservation
		(RoomSessionID, StudentID, ReservationTime, CreatedByID)
	SELECT
		HRS.RoomSessionID, @invitee, GETDATE(), @createdBy
	FROM
		CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID		=	HR.RoomID
									INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
	WHERE
		HRS.HousingYear	=	YEAR(GETDATE())
	AND
		HB.BuildingCode	=	'OAK2'
	AND
		HR.RoomNumber	=	'201B'

ROLLBACK