BEGIN TRANSACTION
	DECLARE	@recreateAllTables		BIT = 0,
			@createdBuilding		BIT = 0,
			@createdRoom			BIT = 0,
			@createdRoomSession		BIT = 0,
			@createdSessionOccupant	BIT = 0,
			@createdRoomReservation	BIT = 0,
			@createdRoomStudent		BIT = 0,
			@createdResidentAssistant	BIT = 0,
			@createdStudentActivity	BIT = 0;

	IF @recreateAllTables = 1
		BEGIN
			DROP TABLE CUS_Housing_StudentActivity;
			DROP TABLE CUS_Housing_ResidentAssistant;
			DROP TABLE CUS_Housing_RoomStudent;
			DROP TABLE CUS_Housing_RoomReservation;
			DROP TABLE CUS_Housing_SessionOccupant;
			DROP TABLE CUS_Housing_RoomSession;
			DROP TABLE CUS_Housing_Room;
			DROP TABLE CUS_Housing_Building;
			PRINT 'Deleted all tables in preparation for recreating them';
		END

	--Check to see if the building table already exists in the database. If not, create it.
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_Building]') AND TYPE IN (N'U'))
		BEGIN
			--Table containing information on residential buildings
			CREATE TABLE dbo.CUS_Housing_Building
			(
				BuildingID		UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
				BuildingName	VARCHAR(255) NOT NULL,
				BuildingCode	VARCHAR(255) NOT NULL
			);
			--Indexes for Building ID and Code
			CREATE INDEX IX_CUS_Housing_Building_ID ON CUS_Housing_Building(BuildingID);
			CREATE INDEX IX_CUS_Housing_Building_Code ON CUS_Housing_Building(BuildingCode);

			PRINT 'Successfully created building table';
			SET @createdBuilding = 1;
		END

	--Check to see if the room table already exists in the database. If not, create it.
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_Room]') AND TYPE IN (N'U'))
		BEGIN
			--Table defining static attributes of residential rooms
			CREATE TABLE dbo.CUS_Housing_Room
			(
				RoomID			UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
				BuildingID		UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CUS_Housing_Building(BuildingID),
				RoomNumber		VARCHAR(10) NOT NULL,
				[Floor]			CHAR(1),
				Wing			VARCHAR(10),
				Capacity		INT NOT NULL DEFAULT 2,
				IsRA			BIT DEFAULT 0,
				DefaultGender	CHAR(1) DEFAULT '',
				DefaultGreek	INT NOT NULL DEFAULT 0
			);
			--Indexes for RoomID and BuildingID
			CREATE INDEX IX_CUS_Housing_Room_Room ON CUS_Housing_Room(RoomID);
			CREATE INDEX IX_CUS_Housing_Room_Building ON CUS_Housing_Room(BuildingID);

			PRINT 'Successfully created room table';
			SET @createdRoom = 1;
		END

	--Check to see if the room session table already exists in the database. If not, create it.
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_RoomSession]') AND TYPE IN (N'U'))
		BEGIN
			--Instance of the room for a specific session. A new set of rooms will be instantiated each year for sign-up
			CREATE TABLE dbo.CUS_Housing_RoomSession
			(
				RoomSessionID	UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
				RoomID			UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CUS_Housing_Room(RoomID),
				Gender			CHAR(1) NOT NULL,
				GreekID			INT NOT NULL DEFAULT 0,
				HousingYear		INT NOT NULL
			);
			--Index for a two-column key on RoomID and HousingYear
			CREATE INDEX IX_CUS_Housing_RoomSession_Room_HousingYear ON CUS_Housing_RoomSession(RoomID, HousingYear);

			PRINT 'Successfully created room/session table';
			SET @createdRoomSession = 1;
		END

	--Check to see if the session occupant table already exists in the database. If not, create it.
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_SessionOccupant]') AND TYPE IN (N'U'))
		BEGIN
			--Details the current (Spring semester) occupant of the room
			CREATE TABLE dbo.CUS_Housing_SessionOccupant
			(
				StudentID		UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES FWK_User(ID),
				RoomSessionID	UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CUS_Housing_RoomSession(RoomSessionID),
				CONSTRAINT pk_HousingSessionOccupant PRIMARY KEY (StudentID, RoomSessionID)
			);
			CREATE INDEX IX_CUS_Housing_SessionOccupant_Student ON CUS_Housing_SessionOccupant(StudentID);
			CREATE INDEX IX_CUS_Housing_SessionOccupant_RoomSession ON CUS_Housing_SessionOccupant(RoomSessionID);

			PRINT 'Successfully created session/occupant table';
			SET @createdSessionOccupant = 1;
		END

	--Check to see if the room reservation table already exists in the database. If not, create it.
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_RoomReservation]') AND TYPE IN (N'U'))
		BEGIN
			CREATE TABLE dbo.CUS_Housing_RoomReservation
			(
				RoomSessionID		UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CUS_Housing_RoomSession(RoomSessionID),
				--The unique identifier of the student who is invited to the room
				StudentID			UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES FWK_User(ID),
				--The date and time when the reservation was created
				ReservationTime		DATETIME NOT NULL DEFAULT GETDATE(),
				--The user who created the reservation
				CreatedByID			UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES FWK_User(ID),
				--Did the student for whom the space was reserved actually register for the bed?
				ReservationAccepted	BIT NOT NULL DEFAULT 0,
				CONSTRAINT pk_HousingRoomReservation PRIMARY KEY (RoomSessionID, StudentID)
			);
			CREATE INDEX IX_CUS_Housing_RoomReservation_RoomSession ON CUS_Housing_RoomReservation(RoomSessionID);
			CREATE INDEX IX_CUS_Housing_RoomReservation_Student ON CUS_Housing_RoomReservation(StudentID);

			PRINT 'Successfully created room reservation table';
			SET @createdRoomReservation = 1;
		END

	--Check to see if the room student table already exists in the database. If not, create it.
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_RoomStudent]') AND TYPE IN (N'U'))
		BEGIN
			--Creates a relationship between the student and the room for which they signed up
			CREATE TABLE dbo.CUS_Housing_RoomStudent
			(
				RoomStudentID		UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
				StudentID			UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES FWK_User(ID),
				RoomSessionID		UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CUS_Housing_RoomSession(RoomSessionID),
				RegistrationTime	DATETIME NOT NULL DEFAULT GETDATE(),
				OaksWaitlist		CHAR(1) NOT NULL DEFAULT 'N'
			);
			CREATE INDEX IX_CUS_Housing_RoomStudent_Student ON CUS_Housing_RoomStudent(StudentID);
			CREATE INDEX IX_CUS_Housing_RoomStudent_RoomSession ON CUS_Housing_RoomStudent(RoomSessionID);

			PRINT 'Successfully created room/student table';
			SET @createdRoomStudent = 1;
		END

	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_ResidentAssistant]') AND TYPE IN (N'U'))
		BEGIN
			CREATE TABLE dbo.CUS_Housing_ResidentAssistant
			(
				ResidentAssistantID	UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
				StudentID			UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES FWK_User(ID),
				RoomSessionID		UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CUS_Housing_RoomSession(RoomSessionID)
			);
			CREATE INDEX IX_CUS_Housing_ResidentAssistant_Student ON CUS_Housing_ResidentAssistant(StudentID);
			CREATE INDEX IX_CUS_Housing_ResidentAssistant_RoomSession ON CUS_Housing_ResidentAssistant(RoomSessionID);
		
			PRINT 'Successfully created resident assistant table';
			SET @createdResidentAssistant = 1;
		END

	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_StudentActivity]') AND TYPE IN (N'U'))
		BEGIN
			CREATE TABLE dbo.CUS_Housing_StudentActivity
			(
				StudentID			UNIQUEIDENTIFIER PRIMARY KEY NOT NULL FOREIGN KEY REFERENCES FWK_User(ID),
				FirstLogin			DATETIME NULL,
				LastLogin			DATETIME NULL,
				TotalLogins			INT NOT NULL DEFAULT 0,
				IsUpperClassman		BIT NOT NULL DEFAULT 0
			);
			CREATE INDEX IX_CUS_Housing_StudentActivity_Student ON CUS_Housing_StudentActivity(StudentID);
			PRINT 'Successfully created student activity table';
			SET @createdStudentActivity = 1;
		END
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUS_Housing_Settings]') AND TYPE IN (N'U'))
		BEGIN
			CREATE TABLE dbo.CUS_Housing_Settings
			(
				SettingKey		VARCHAR(255) PRIMARY KEY NOT NULL,
				SettingValue	VARCHAR(1000) NOT NULL
			);
			CREATE INDEX IX_CUS_Housing_Settings_Key ON CUS_Housing_Settings(SettingKey);
			PRINT 'Successfully created housing settings table';
		END
ROLLBACK