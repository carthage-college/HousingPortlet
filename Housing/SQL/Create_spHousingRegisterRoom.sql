SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Mike Kishline
-- Create date: 3/25/2015
-- Last update:	4/7/2015
-- Description:	Attempts to place a student into a specific room while factoring in room capacity and gender.
--				If the placement is successful, the gender of the room is updated (if it is not already defined)
--				to prevent coed habitation.
--				If the placement is successful, delete any remaining invitations that were extended to the student.
--				A numeric response code is sent back to the calling application so the results can be interpreted
--				and displayed to the user.
-- Return values:	 0: Success
--					-1: Gender of the room did not match the student's gender
--					-2: The capacity of the room has already been reached
--					-3: The student has already registered for a room
--					-4: Invalid gender was passed to the procedure
--					-5: Unknown error
-- =============================================

--If this stored procedure already exists in the database, remove and recreate it
IF OBJECT_ID('dbo.CUS_spHousingRegisterRoom', 'P') IS NOT NULL
    DROP PROCEDURE dbo.CUS_spHousingRegisterRoom;
GO

CREATE PROCEDURE [dbo].[CUS_spHousingRegisterRoom]
	--The FWK_User.ID value of the student
	@uuidStudentID		UNIQUEIDENTIFIER,
	--The value from CUS_Housing_RoomSession.RoomSessionID denoting a specific room for a single housing year
	@uuidRoomSessionID	UNIQUEIDENTIFIER,
	--The gender of the student
	@strGender			CHAR(1)	=	'',
	--Does the student wish to be placed on a waitlist for the Oaks?
	@strOaksWaitlist	CHAR(1)	=	'N'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON;

	PRINT 'Check for valid student gender';
	IF @strGender NOT IN ('M','F')
		BEGIN
			SELECT -4 AS returnCode
			RETURN
		END
	PRINT 'Gender argument ' + @strGender + ' is valid';

	--Count the number of instances the student's ID appears for the current year in the table tracking sign ups.
	DECLARE	@intStudentBedCount			INT	=	(
		SELECT
			COUNT(*)
		FROM
			CUS_Housing_RoomStudent	HRStu	INNER JOIN	CUS_Housing_RoomSession	HRS	ON	HRStu.RoomSessionID	=	HRS.RoomSessionID
		WHERE
			HRStu.StudentID	=	@uuidStudentID
		AND
			HRS.HousingYear	=	YEAR(GETDATE())
	);
	PRINT 'Student bed count: ' + CAST(@intStudentBedCount AS VARCHAR(3));
	--If the student has already signed up for a bed in this housing period, abort the process
	IF @intStudentBedCount > 0
		BEGIN
			SELECT -3 AS returnCode
			RETURN
		END
	PRINT 'Student has not already been assigned to a bed.';

	--Create unique identifier to serve as primary key for insert into CUS_Housing_RoomStudent
	DECLARE	@uuidRoomStudentID	UNIQUEIDENTIFIER	=	NEWID();
	PRINT 'New RoomStudentID: ' + CAST(@uuidRoomStudentID AS VARCHAR(100));

	PRINT 'Attempt to insert new record into CUS_Housing_RoomStudent';
	--Attempt the insert with gender and capacity values considered
	INSERT INTO
		CUS_Housing_RoomStudent (RoomStudentID, StudentID, RoomSessionID, OaksWaitlist)
	SELECT
		@uuidRoomStudentID, @uuidStudentID, HRS.RoomSessionID, @strOaksWaitlist
	FROM
		CUS_Housing_Room	HR	INNER JOIN	CUS_Housing_RoomSession	HRS		ON	HR.RoomID			=	HRS.RoomID
								LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
								INNER JOIN	CUS_Housing_Building	HB		ON	HR.BuildingID		=	HB.BuildingID
	WHERE
		HRS.RoomSessionID	=	@uuidRoomSessionID
	AND
		--Is the gender of the room either blank (unassigned) or a match to the student's gender
		HRS.Gender			IN	('', @strGender)
	GROUP BY
		HRS.RoomSessionID, HR.Capacity
	--Is there still at least one available bed in the room?
	HAVING
		COUNT(HRStu.RoomStudentID)	<	HR.Capacity;


	--Check to see if the unique identifier (@uuidRoomStudentID) exists which indicates success
	DECLARE	@intSuccess	INT	=	(
		SELECT
			COUNT(*)
		FROM
			CUS_Housing_RoomStudent
		WHERE
			CUS_Housing_RoomStudent.RoomStudentID	=	@uuidRoomStudentID
	);
	IF @intSuccess	=	1
		BEGIN
			PRINT 'The record was inserted successfully. Update the gender for the room, if necessary.';
			--If the record was successfully added, update the gender of the room if it is not already defined
			UPDATE
				CUS_Housing_RoomSession
			SET
				Gender			=	@strGender
			WHERE
				RoomSessionID	=	@uuidRoomSessionID
			AND
				Gender			=	''

			--For the Oaks: If the gender of both rooms in the suite have not already been set, find the corresponding suite and update the gender to match the student who just reserved the other suite
			--NOTE!! - This UPDATE should affect 0 rows for all rooms that are not a suite in the Oaks
			UPDATE
				HRS
			SET
				HRS.Gender		=	@strGender
			FROM
				CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room	HR	ON	HRS.RoomID		=	HR.RoomID
																				AND	HRS.HousingYear	=	YEAR(GETDATE()),
											(
												--Gets the information about the room that was just registered
												SELECT
													HRSsub.RoomSessionID, HRsub.BuildingID, SUBSTRING(HRsub.RoomNumber, 1, 3) AS RoomNumberOnly
												FROM
													CUS_Housing_Room	HRsub	INNER JOIN	CUS_Housing_RoomSession	HRSsub	ON	HRsub.RoomID	=	HRSsub.RoomID
												WHERE
													HRSsub.RoomSessionID	=	@uuidRoomSessionID
											)	RegisteredRoom
			WHERE
				HR.BuildingID					=	RegisteredRoom.BuildingID
			AND
				SUBSTRING(HR.RoomNumber, 1, 3)	=	RegisteredRoom.RoomNumberOnly
			AND
				HRS.RoomSessionID				<>	RegisteredRoom.RoomSessionID
			AND
				HRS.Gender						=	''

			PRINT 'Delete any outstanding invitations for the student';
			DELETE FROM
				CUS_Housing_RoomReservation
			WHERE
				CUS_Housing_RoomReservation.StudentID	=	@uuidStudentID

			SELECT 0 AS returnCode
			RETURN
		END
	ELSE
		BEGIN
			PRINT 'There was a problem when attempting to insert the record. Begin process to uncover source of the problem.';
			PRINT '@intSuccess: ' + CAST(@intSuccess AS VARCHAR(6));
			DECLARE	@intBedsWithDifferentGender INT	=	(SELECT COUNT(*) FROM CUS_Housing_RoomSession HRS WHERE HRS.RoomSessionID = @uuidRoomSessionID AND HRS.Gender <> @strGender);
			DECLARE	@intBedsFilled				INT	=	(SELECT COUNT(*) FROM CUS_Housing_RoomStudent HRS WHERE HRS.RoomSessionID = @uuidRoomSessionID);
			DECLARE	@intRoomCapacity			INT	=	(SELECT HR.Capacity FROM CUS_Housing_Room HR INNER JOIN CUS_Housing_RoomSession HRS ON HR.RoomID = HRS.RoomID WHERE HRS.RoomSessionID = @uuidRoomSessionID);
			--Was the problem a gender mismatch?
			IF @intBedsWithDifferentGender > 0
				BEGIN
					SELECT -1 AS returnCode
					RETURN
				END
			PRINT 'The gender of the room was not the problem.'
			--Was the room full?
			IF @intBedsFilled >= @intRoomCapacity
				BEGIN
					SELECT -2 AS returnCode
					RETURN
				END
			PRINT 'There were still available beds in the room.'
			--We don't know what went wrong, but the record was not inserted
			SELECT -5 AS returnCode
		END
END
GO