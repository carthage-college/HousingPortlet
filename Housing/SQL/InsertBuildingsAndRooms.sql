USE ICS_NET_TEST

BEGIN TRANSACTION
	DECLARE	@clearExistingData	BIT	=	1;

	IF @clearExistingData = 1
		BEGIN
			DELETE FROM dbo.CUS_Housing_Room;
			DELETE FROM dbo.CUS_Housing_Building;
			PRINT 'Deleted data from CUS_Housing_Room and CUS_Housing_Building';
		END

	--Create the campus
	INSERT INTO dbo.CUS_Housing_Building (BuildingID, BuildingName, BuildingCode)
	VALUES
		(NEWID(), 'Campus Apartments', 'APT'),	(NEWID(), 'Denhart', 'DEN'),
		(NEWID(), 'Johnson', 'JOH'),			(NEWID(), 'Madrigrano', 'MADR'),
		(NEWID(), 'Oaks 1', 'OAK1'),			(NEWID(), 'Oaks 2', 'OAK2'),
		(NEWID(), 'Oaks 3', 'OAK3'),			(NEWID(), 'Oaks 4', 'OAK4'),
		(NEWID(), 'Oaks 5', 'OAK5'),			(NEWID(), 'Oaks 6', 'OAK6'),
		(NEWID(), 'Swenson', 'SWE'),			(NEWID(), 'Tarble', 'TAR');

	DECLARE	@buildingsCreated INT	=	(SELECT COUNT(*) FROM CUS_Housing_Building);
	PRINT 'Created ' + CAST(@buildingsCreated AS VARCHAR) + ' buildings';

	DECLARE @apt	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'APT'),
			@den	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'DEN'),
			@joh	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'JOH'),
			@madr	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'MADR'),
			@oak1	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'OAK1'),
			@oak2	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'OAK2'),
			@oak3	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'OAK3'),
			@oak4	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'OAK4'),
			@oak5	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'OAK5'),
			@oak6	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'OAK6'),
			@swe	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'SWE'),
			@tar	UNIQUEIDENTIFIER	=	(SELECT BuildingID FROM CUS_Housing_Building WHERE BuildingCode = 'TAR');

	--Create Rooms for campus
	/*
		--Values created by running the following SQL against informix
		--Make sure to create a new INSERT statement at 1000 rows, which is the maximum number of inserts that can be performed with a single statement
		SELECT
			bldg, room, SUM(max_occ) AS total_occ,
			"(NewID(), @" || LOWER(TRIM(bldg)) || ", '" || TRIM(room) || "', '" || CASE NVL(floor,'') WHEN '' THEN room[1,1] ELSE floor END || "', '" || CASE NVL(wing,'') WHEN '' THEN room[1,1] ELSE wing END || "', " || SUM(max_occ) || ", '" || TRIM(NVL(gender,'')) || "'), " AS sql
		FROM
			facil_table
		WHERE
			ctgry = 'D'
		AND
			bldg IN ('APT','DEN','JOH','MADR','OAK1','OAK2','OAK3','OAK4','OAK5','OAK6','SWE','TAR')
		AND
			room != ('UN')
		AND
			TODAY BETWEEN active_date AND NVL(inactive_date, TODAY)
		GROUP BY
			bldg, room, floor, wing, gender
		ORDER BY
			bldg, room
	*/
	INSERT INTO dbo.CUS_Housing_Room (RoomID, BuildingID, RoomNumber, [Floor], Wing, Capacity, DefaultGender)
	VALUES
		--Campus Apartments (8 rooms)
		(NewID(), @apt , '001', '1', '1', 3, ''), (NewID(), @apt , '002', '1', '1', 3, ''), (NewID(), @apt , '003', '1', '1', 3, ''), (NewID(), @apt , '004', '1', '1', 3, ''), (NewID(), @apt , '005', '1', '1', 3, ''),
		(NewID(), @apt , '006', '1', '1', 3, ''), (NewID(), @apt , '007', '1', '1', 3, ''), (NewID(), @apt , '008', '1', '1', 3, ''),
		--Denhart
		(NewID(), @den , '005', 'T', 'A', 3, 'M'), (NewID(), @den , '006', 'T', 'A', 2, 'M'), (NewID(), @den , '009', 'T', 'A', 3, 'M'), (NewID(), @den , '010', 'T', 'A', 2, 'M'), (NewID(), @den , '011', 'T', 'A', 2, 'M'),
		(NewID(), @den , '012', 'T', 'A', 1, 'M'), (NewID(), @den , '013', 'T', 'A', 2, 'M'), (NewID(), @den , '014', 'T', 'A', 4, 'M'), (NewID(), @den , '015', 'T', 'A', 4, 'M'), (NewID(), @den , '016', 'S', 'B', 3, 'F'),
		(NewID(), @den , '017', 'S', 'B', 3, 'F'), (NewID(), @den , '018', 'S', 'B', 3, 'F'), (NewID(), @den , '019', 'S', 'B', 3, 'F'), (NewID(), @den , '020', 'S', 'B', 2, 'F'), (NewID(), @den , '021', 'S', 'B', 3, 'F'),
		(NewID(), @den , '022', 'S', 'B', 3, 'F'), (NewID(), @den , '023', 'S', 'B', 3, 'F'), (NewID(), @den , '100', '1', 'A', 2, 'M'), (NewID(), @den , '101', '1', 'A', 2, 'M'), (NewID(), @den , '102', '1', 'A', 2, 'M'),
		(NewID(), @den , '103', '1', 'A', 2, 'M'), (NewID(), @den , '104', '1', 'A', 2, 'M'), (NewID(), @den , '105', '1', 'A', 2, 'M'), (NewID(), @den , '106', '1', 'A', 2, 'M'), (NewID(), @den , '107', '1', 'A', 2, 'M'),
		(NewID(), @den , '108', '1', 'A', 1, 'M'), (NewID(), @den , '109', '1', 'A', 2, 'M'), (NewID(), @den , '110', '1', 'A', 2, 'M'), (NewID(), @den , '111', '1', 'A', 2, 'M'), (NewID(), @den , '112', '1', 'A', 2, 'M'),
		(NewID(), @den , '113', '1', 'A', 2, 'M'), (NewID(), @den , '115', '1', 'A', 2, 'M'), (NewID(), @den , '149', '1', 'B', 2, 'F'), (NewID(), @den , '150', '1', 'B', 2, 'F'), (NewID(), @den , '151', '1', 'B', 1, 'F'),
		(NewID(), @den , '152', '1', 'B', 2, 'F'), (NewID(), @den , '153', '1', 'B', 2, 'F'), (NewID(), @den , '154', '1', 'B', 2, 'F'), (NewID(), @den , '155', '1', 'B', 2, 'F'), (NewID(), @den , '156', '1', 'B', 2, 'F'),
		(NewID(), @den , '157', '1', 'B', 2, 'F'), (NewID(), @den , '158', '1', 'B', 2, 'F'), (NewID(), @den , '159', '1', 'B', 2, 'F'), (NewID(), @den , '160', '1', 'B', 2, 'F'), (NewID(), @den , '161', '1', 'B', 2, 'F'),
		(NewID(), @den , '162', '1', 'B', 2, 'F'), (NewID(), @den , '163', '1', 'B', 2, 'F'), (NewID(), @den , '164', '1', 'B', 2, 'F'), (NewID(), @den , '165', '1', 'B', 2, 'F'), (NewID(), @den , '166', '1', 'B', 4, 'F'),
		(NewID(), @den , '200', '2', 'A', 2, 'F'), (NewID(), @den , '201', '2', 'A', 2, 'F'), (NewID(), @den , '202', '2', 'A', 2, 'F'), (NewID(), @den , '203', '2', 'A', 2, 'F'), (NewID(), @den , '204', '2', 'A', 2, 'F'),
		(NewID(), @den , '205', '2', 'A', 2, 'F'), (NewID(), @den , '206', '2', 'A', 2, 'F'), (NewID(), @den , '207', '2', 'A', 2, 'F'), (NewID(), @den , '208', '2', 'A', 2, 'F'), (NewID(), @den , '209', '2', 'A', 2, 'F'),
		(NewID(), @den , '210', '2', 'A', 2, 'F'), (NewID(), @den , '211', '2', 'A', 2, 'F'), (NewID(), @den , '212', '2', 'A', 1, 'F'), (NewID(), @den , '213', '2', 'A', 2, 'F'), (NewID(), @den , '214', '2', 'A', 2, 'F'),
		(NewID(), @den , '215', '2', 'A', 2, 'F'), (NewID(), @den , '216', '2', 'A', 2, 'F'), (NewID(), @den , '218', '2', 'A', 2, 'F'), (NewID(), @den , '220', '2', 'A', 2, 'F'), (NewID(), @den , '249', '2', 'B', 2, 'F'),
		(NewID(), @den , '250', '2', 'B', 2, 'F'), (NewID(), @den , '251', '2', 'B', 1, 'F'), (NewID(), @den , '252', '2', 'B', 2, 'F'), (NewID(), @den , '253', '2', 'B', 2, 'F'), (NewID(), @den , '254', '2', 'B', 2, 'F'),
		(NewID(), @den , '255', '2', 'B', 2, 'F'), (NewID(), @den , '256', '2', 'B', 2, 'F'), (NewID(), @den , '257', '2', 'B', 2, 'F'), (NewID(), @den , '258', '2', 'B', 2, 'F'), (NewID(), @den , '259', '2', 'B', 2, 'F'),
		(NewID(), @den , '260', '2', 'B', 2, 'F'), (NewID(), @den , '261', '2', 'B', 2, 'F'), (NewID(), @den , '262', '2', 'B', 2, 'F'), (NewID(), @den , '263', '2', 'B', 2, 'F'), (NewID(), @den , '264', '2', 'B', 2, 'F'),
		(NewID(), @den , '265', '2', 'B', 2, 'F'), (NewID(), @den , '266', '2', 'B', 2, 'F'), (NewID(), @den , '268', '2', 'B', 2, 'F'), (NewID(), @den , '300', '3', 'A', 2, 'M'), (NewID(), @den , '301', '3', 'A', 2, 'M'),
		(NewID(), @den , '302', '3', 'A', 2, 'M'), (NewID(), @den , '303', '3', 'A', 2, 'M'), (NewID(), @den , '304', '3', 'A', 2, 'M'), (NewID(), @den , '305', '3', 'A', 2, 'M'), (NewID(), @den , '306', '3', 'A', 2, 'M'),
		(NewID(), @den , '307', '3', 'A', 2, 'M'), (NewID(), @den , '308', '3', 'A', 1, 'M'), (NewID(), @den , '309', '3', 'A', 2, 'M'), (NewID(), @den , '310', '3', 'A', 2, 'M'), (NewID(), @den , '311', '3', 'A', 2, 'M'),
		(NewID(), @den , '312', '3', 'A', 2, 'M'), (NewID(), @den , '313', '3', 'A', 2, 'M'), (NewID(), @den , '314', '3', 'A', 2, 'M'), (NewID(), @den , '315', '3', 'A', 2, 'M'), (NewID(), @den , '316', '3', 'A', 2, 'M'),
		(NewID(), @den , '318', '3', 'A', 2, 'M'), (NewID(), @den , '320', '3', 'A', 1, 'F'), (NewID(), @den , '349', '3', 'B', 2, 'M'), (NewID(), @den , '350', '3', 'B', 2, 'M'), (NewID(), @den , '351', '3', 'B', 1, 'M'),
		(NewID(), @den , '352', '3', 'B', 2, 'M'), (NewID(), @den , '353', '3', 'B', 2, 'M'), (NewID(), @den , '354', '3', 'B', 2, 'M'), (NewID(), @den , '355', '3', 'B', 2, 'M'), (NewID(), @den , '356', '3', 'B', 2, 'M'),
		(NewID(), @den , '357', '3', 'B', 2, 'M'), (NewID(), @den , '358', '3', 'B', 2, 'M'), (NewID(), @den , '359', '3', 'B', 2, 'M'), (NewID(), @den , '360', '3', 'B', 2, 'M'), (NewID(), @den , '361', '3', 'B', 2, 'M'),
		(NewID(), @den , '362', '3', 'B', 2, 'M'), (NewID(), @den , '363', '3', 'B', 2, 'M'), (NewID(), @den , '364', '3', 'B', 2, 'M'), (NewID(), @den , '365', '3', 'B', 2, 'M'), (NewID(), @den , '366', '3', 'B', 2, 'M'),
		(NewID(), @den , '368', '3', 'B', 2, 'M'), (NewID(), @den , '400', '4', 'A', 2, 'F'), (NewID(), @den , '401', '4', 'A', 2, 'F'), (NewID(), @den , '402', '4', 'A', 2, 'F'), (NewID(), @den , '403', '4', 'A', 2, 'F'),
		(NewID(), @den , '404', '4', 'A', 2, 'F'), (NewID(), @den , '405', '4', 'A', 2, 'F'), (NewID(), @den , '406', '4', 'A', 2, 'F'), (NewID(), @den , '407', '4', 'A', 2, 'F'), (NewID(), @den , '408', '4', 'A', 1, 'F'),
		(NewID(), @den , '409', '4', 'A', 2, 'F'), (NewID(), @den , '410', '4', 'A', 2, 'F'), (NewID(), @den , '411', '4', 'A', 2, 'F'), (NewID(), @den , '412', '4', 'A', 2, 'F'), (NewID(), @den , '413', '4', 'A', 2, 'F'),
		(NewID(), @den , '414', '4', 'A', 2, 'F'), (NewID(), @den , '415', '4', 'A', 2, 'F'), (NewID(), @den , '416', '4', 'A', 2, 'F'), (NewID(), @den , '418', '4', 'A', 2, 'F'), (NewID(), @den , '420', '4', 'A', 2, 'F'),
		(NewID(), @den , '449', '4', 'B', 2, ''), (NewID(), @den , '450', '4', 'B', 2, 'M'), (NewID(), @den , '451', '4', 'B', 1, 'M'), (NewID(), @den , '452', '4', 'B', 2, 'M'), (NewID(), @den , '453', '4', 'B', 2, 'M'),
		(NewID(), @den , '454', '4', 'B', 2, 'M'), (NewID(), @den , '455', '4', 'B', 2, 'M'), (NewID(), @den , '456', '4', 'B', 2, 'M'), (NewID(), @den , '457', '4', 'B', 2, 'M'), (NewID(), @den , '458', '4', 'B', 2, 'M'),
		(NewID(), @den , '459', '4', 'B', 2, 'M'), (NewID(), @den , '460', '4', 'B', 2, 'M'), (NewID(), @den , '461', '4', 'B', 2, 'M'), (NewID(), @den , '462', '4', 'B', 2, 'M'), (NewID(), @den , '463', '4', 'B', 2, 'M'),
		(NewID(), @den , '464', '4', 'B', 2, 'M'), (NewID(), @den , '465', '4', 'B', 2, 'M'), (NewID(), @den , '466', '4', 'B', 2, 'M'), (NewID(), @den , '468', '4', 'B', 2, 'M'),
		--Johnson
		(NewID(), @joh , '003', 'S', 'B', 4, 'M'), (NewID(), @joh , '004', 'S', 'B', 3, 'M'), (NewID(), @joh , '005', 'S', 'B', 3, 'M'), (NewID(), @joh , '006', 'S', 'B', 3, 'M'), (NewID(), @joh , '007', 'S', 'B', 3, 'M'),
		(NewID(), @joh , '008', 'S', 'B', 2, 'M'), (NewID(), @joh , '009', 'S', 'B', 4, 'M'), (NewID(), @joh , '010', 'T', 'A', 2, 'M'), (NewID(), @joh , '011', 'T', 'A', 2, 'M'), (NewID(), @joh , '012', 'T', 'A', 2, 'M'),
		(NewID(), @joh , '013', 'T', 'A', 2, 'M'), (NewID(), @joh , '014', 'T', 'A', 3, 'M'), (NewID(), @joh , '015', 'T', 'A', 2, 'M'), (NewID(), @joh , '016', 'T', 'A', 2, 'M'), (NewID(), @joh , '100', '1', 'A', 2, 'M'),
		(NewID(), @joh , '101', '1', 'A', 2, 'M'), (NewID(), @joh , '102', '1', 'A', 2, 'M'), (NewID(), @joh , '103', '1', 'A', 2, 'M'), (NewID(), @joh , '104', '1', 'A', 2, 'M'), (NewID(), @joh , '105', '1', 'A', 2, 'M'),
		(NewID(), @joh , '106', '1', 'A', 2, 'M'), (NewID(), @joh , '107', '1', 'A', 2, 'M'), (NewID(), @joh , '108', '1', 'A', 2, 'M'), (NewID(), @joh , '110', '1', 'A', 1, 'M'), (NewID(), @joh , '112', '1', 'A', 2, 'M'),
		(NewID(), @joh , '114', '1', 'A', 2, 'M'), (NewID(), @joh , '116', '1', 'A', 2, 'M'), (NewID(), @joh , '118', '1', 'A', 2, 'M'), (NewID(), @joh , '120', '1', 'A', 2, 'M'), (NewID(), @joh , '150', '1', 'B', 2, 'M'),
		(NewID(), @joh , '151', '1', 'B', 2, 'M'), (NewID(), @joh , '152', '1', 'B', 2, 'M'), (NewID(), @joh , '153', '1', 'B', 2, 'M'), (NewID(), @joh , '154', '1', 'B', 2, 'M'), (NewID(), @joh , '155', '1', 'B', 2, 'M'),
		(NewID(), @joh , '156', '1', 'B', 2, 'M'), (NewID(), @joh , '157', '1', 'B', 2, 'M'), (NewID(), @joh , '158', '1', 'B', 1, 'M'), (NewID(), @joh , '159', '1', 'B', 2, 'M'), (NewID(), @joh , '160', '1', 'B', 2, 'M'),
		(NewID(), @joh , '161', '1', 'B', 2, 'M'), (NewID(), @joh , '162', '1', 'B', 2, 'M'), (NewID(), @joh , '163', '1', 'B', 2, 'M'), (NewID(), @joh , '164', '1', 'B', 2, 'M'), (NewID(), @joh , '165', '1', 'B', 2, 'M'),
		(NewID(), @joh , '166', '1', 'B', 2, 'M'), (NewID(), @joh , '168', '1', 'B', 2, 'M'), (NewID(), @joh , '200', '2', 'A', 2, 'F'), (NewID(), @joh , '201', '2', 'A', 2, 'F'), (NewID(), @joh , '202', '2', 'A', 2, 'F'),
		(NewID(), @joh , '203', '2', 'A', 2, 'F'), (NewID(), @joh , '204', '2', 'A', 2, 'F'), (NewID(), @joh , '205', '2', 'A', 2, 'F'), (NewID(), @joh , '206', '2', 'A', 2, 'F'), (NewID(), @joh , '207', '2', 'A', 2, 'F'),
		(NewID(), @joh , '208', '2', 'A', 1, 'F'), (NewID(), @joh , '209', '2', 'A', 2, 'F'), (NewID(), @joh , '210', '2', 'A', 2, 'F'), (NewID(), @joh , '211', '2', 'A', 2, 'F'), (NewID(), @joh , '212', '2', 'A', 2, 'F'),
		(NewID(), @joh , '213', '2', 'A', 2, 'F'), (NewID(), @joh , '214', '2', 'A', 2, 'F'), (NewID(), @joh , '215', '2', 'A', 2, 'F'), (NewID(), @joh , '216', '2', 'A', 2, 'F'), (NewID(), @joh , '218', '2', 'A', 2, 'F'),
		(NewID(), @joh , '220', '2', 'A', 2, 'F'), (NewID(), @joh , '222', '2', 'A', 4, 'F'), (NewID(), @joh , '250', '2', 'B', 2, 'F'), (NewID(), @joh , '251', '2', 'B', 2, 'F'), (NewID(), @joh , '252', '2', 'B', 2, 'F'),
		(NewID(), @joh , '253', '2', 'B', 2, ''), (NewID(), @joh , '254', '2', 'B', 2, 'F'), (NewID(), @joh , '255', '2', 'B', 2, 'F'), (NewID(), @joh , '256', '2', 'B', 2, 'F'), (NewID(), @joh , '257', '2', 'B', 2, 'F'),
		(NewID(), @joh , '258', '2', 'B', 1, 'F'), (NewID(), @joh , '259', '2', 'B', 2, 'F'), (NewID(), @joh , '260', '2', 'B', 2, 'F'), (NewID(), @joh , '261', '2', 'B', 2, 'F'), (NewID(), @joh , '262', '2', 'B', 2, 'F'),
		(NewID(), @joh , '263', '2', 'B', 2, 'F'), (NewID(), @joh , '264', '2', 'B', 2, 'F'), (NewID(), @joh , '265', '2', 'B', 2, 'F'), (NewID(), @joh , '266', '2', 'B', 2, 'F'), (NewID(), @joh , '268', '2', 'B', 2, 'F'),
		(NewID(), @joh , '300', '3', 'A', 2, 'M'), (NewID(), @joh , '301', '3', 'A', 2, 'M'), (NewID(), @joh , '302', '3', 'A', 2, 'M'), (NewID(), @joh , '303', '3', 'A', 2, 'M'), (NewID(), @joh , '304', '3', 'A', 2, 'M'),
		(NewID(), @joh , '305', '3', 'A', 2, 'M'), (NewID(), @joh , '306', '3', 'A', 2, 'M'), (NewID(), @joh , '307', '3', 'A', 2, 'M'), (NewID(), @joh , '308', '3', 'A', 1, 'M'), (NewID(), @joh , '309', '3', 'A', 2, 'M'),
		(NewID(), @joh , '310', '3', 'A', 2, 'M'), (NewID(), @joh , '311', '3', 'A', 2, 'M'), (NewID(), @joh , '312', '3', 'A', 2, 'M'), (NewID(), @joh , '313', '3', 'A', 2, 'M'), (NewID(), @joh , '314', '3', 'A', 2, 'M'),
		(NewID(), @joh , '315', '3', 'A', 2, 'M'), (NewID(), @joh , '316', '3', 'A', 2, 'M'), (NewID(), @joh , '318', '3', 'A', 2, 'M'), (NewID(), @joh , '320', '3', 'A', 2, 'M'), (NewID(), @joh , '322', '3', 'A', 4, 'M'),
		(NewID(), @joh , '350', '3', 'B', 2, 'M'), (NewID(), @joh , '351', '3', 'B', 1, ''), (NewID(), @joh , '352', '3', 'B', 2, 'M'), (NewID(), @joh , '353', '3', 'B', 2, 'M'), (NewID(), @joh , '354', '3', 'B', 2, 'M'),
		(NewID(), @joh , '355', '3', 'B', 2, ''), (NewID(), @joh , '356', '3', 'B', 2, 'M'), (NewID(), @joh , '357', '3', 'B', 2, 'M'), (NewID(), @joh , '358', '3', 'B', 1, 'M'), (NewID(), @joh , '359', '3', 'B', 2, 'M'),
		(NewID(), @joh , '360', '3', 'B', 2, 'M'), (NewID(), @joh , '361', '3', 'B', 2, 'M'), (NewID(), @joh , '362', '3', 'B', 2, 'M'), (NewID(), @joh , '363', '3', 'B', 2, 'M'), (NewID(), @joh , '364', '3', 'B', 2, 'M'),
		(NewID(), @joh , '365', '3', 'B', 2, 'M'), (NewID(), @joh , '366', '3', 'B', 2, 'M'), (NewID(), @joh , '368', '3', 'B', 2, 'M'), (NewID(), @joh , '400', '4', 'A', 2, 'M'), (NewID(), @joh , '401', '4', 'A', 2, 'M'),
		(NewID(), @joh , '402', '4', 'A', 2, 'M'), (NewID(), @joh , '403', '4', 'A', 2, 'M'), (NewID(), @joh , '404', '4', 'A', 2, 'M'), (NewID(), @joh , '405', '4', 'A', 2, 'M'), (NewID(), @joh , '406', '4', 'A', 2, ''),
		(NewID(), @joh , '407', '4', 'A', 2, 'M'), (NewID(), @joh , '408', '4', 'A', 2, 'M'), (NewID(), @joh , '409', '4', 'A', 2, 'M'), (NewID(), @joh , '410', '4', 'A', 1, 'M'), (NewID(), @joh , '411', '4', 'A', 2, ''),
		(NewID(), @joh , '412', '4', 'A', 2, 'M'), (NewID(), @joh , '413', '4', 'A', 2, 'M'), (NewID(), @joh , '414', '4', 'A', 2, 'M'), (NewID(), @joh , '415', '4', 'A', 2, 'M'), (NewID(), @joh , '416', '4', 'A', 2, ''),
		(NewID(), @joh , '418', '4', 'A', 2, 'M'), (NewID(), @joh , '420', '4', 'A', 2, 'M'), (NewID(), @joh , '422', '4', 'A', 4, 'M'), (NewID(), @joh , '450', '4', 'B', 2, 'F'), (NewID(), @joh , '451', '4', 'B', 2, 'F'),
		(NewID(), @joh , '452', '4', 'B', 2, 'F'), (NewID(), @joh , '453', '4', 'B', 2, 'F'), (NewID(), @joh , '454', '4', 'B', 2, 'F'), (NewID(), @joh , '455', '4', 'B', 2, 'F'), (NewID(), @joh , '456', '4', 'B', 2, 'F'),
		(NewID(), @joh , '457', '4', 'B', 2, 'F'), (NewID(), @joh , '458', '4', 'B', 2, 'F'), (NewID(), @joh , '459', '4', 'B', 2, 'F'), (NewID(), @joh , '460', '4', 'B', 1, 'F'), (NewID(), @joh , '461', '4', 'B', 2, 'F'),
		(NewID(), @joh , '462', '4', 'B', 2, 'F'), (NewID(), @joh , '463', '4', 'B', 2, 'F'), (NewID(), @joh , '464', '4', 'B', 2, 'F'), (NewID(), @joh , '465', '4', 'B', 2, 'F'), (NewID(), @joh , '466', '4', 'B', 2, 'F'),
		(NewID(), @joh , '468', '4', 'B', 2, 'F'),
		--Madrigrano
		(NewID(), @madr, '001', 'T', 'B', 3, 'F'), (NewID(), @madr, '002', 'T', 'B', 2, 'F'), (NewID(), @madr, '003', 'T', 'B', 2, 'F'), (NewID(), @madr, '128', '1', 'A', 2, 'M'), (NewID(), @madr, '130', '1', 'A', 2, 'M'),
		(NewID(), @madr, '132', '1', 'A', 2, 'M'), (NewID(), @madr, '134', '1', 'A', 2, 'M'), (NewID(), @madr, '136', '1', 'A', 2, 'M'), (NewID(), @madr, '138', '1', 'A', 2, 'M'), (NewID(), @madr, '139', '1', 'A', 2, 'M'),
		(NewID(), @madr, '140', '1', 'A', 1, 'M'), (NewID(), @madr, '141', '1', 'A', 2, 'M'), (NewID(), @madr, '142', '1', 'A', 2, 'M'), (NewID(), @madr, '143', '1', 'A', 2, 'M'), (NewID(), @madr, '144', '1', 'A', 2, 'M'),
		(NewID(), @madr, '145', '1', 'A', 2, 'M'), (NewID(), @madr, '146', '1', 'A', 2, 'M'), (NewID(), @madr, '147', '1', 'A', 2, 'M'), (NewID(), @madr, '149', '1', 'A', 2, 'M'), (NewID(), @madr, '160', '1', 'B', 2, 'F'),
		(NewID(), @madr, '162', '1', 'B', 2, 'F'), (NewID(), @madr, '164', '1', 'B', 2, 'F'), (NewID(), @madr, '165', '1', 'B', 2, 'F'), (NewID(), @madr, '166', '1', 'B', 2, 'F'), (NewID(), @madr, '167', '1', 'B', 2, 'F'),
		(NewID(), @madr, '168', '1', 'B', 2, 'F'), (NewID(), @madr, '169', '1', 'B', 2, 'F'), (NewID(), @madr, '170', '1', 'B', 2, 'F'), (NewID(), @madr, '172', '1', 'B', 2, 'F'), (NewID(), @madr, '174', '1', 'B', 1, 'F'),
		(NewID(), @madr, '175', '1', 'B', 2, 'F'), (NewID(), @madr, '176', '1', 'B', 2, 'F'), (NewID(), @madr, '177', '1', 'B', 2, 'F'), (NewID(), @madr, '178', '1', 'B', 2, 'F'), (NewID(), @madr, '179', '1', 'B', 2, 'F'),
		(NewID(), @madr, '180', '1', 'B', 2, 'F'), (NewID(), @madr, '181', '1', 'B', 2, 'F'), (NewID(), @madr, '182', '1', 'B', 2, 'F'), (NewID(), @madr, '184', '1', 'B', 2, 'F'), (NewID(), @madr, '222', '2', 'B', 2, 'M'),
		(NewID(), @madr, '228', '2', 'A', 2, 'M'), (NewID(), @madr, '230', '2', 'A', 2, 'M'), (NewID(), @madr, '231', '2', 'A', 2, 'M'), (NewID(), @madr, '232', '2', 'A', 2, 'M'), (NewID(), @madr, '233', '2', 'A', 2, 'M'),
		(NewID(), @madr, '234', '2', 'A', 2, 'M'), (NewID(), @madr, '235', '2', 'A', 2, 'M'), (NewID(), @madr, '236', '2', 'A', 2, 'M'), (NewID(), @madr, '237', '2', 'A', 2, 'M'), (NewID(), @madr, '238', '2', 'A', 2, 'M'),
		(NewID(), @madr, '239', '2', 'A', 2, 'M'), (NewID(), @madr, '240', '2', 'A', 1, 'M'), (NewID(), @madr, '241', '2', 'A', 2, 'M'), (NewID(), @madr, '242', '2', 'A', 2, 'M'), (NewID(), @madr, '243', '2', 'A', 2, 'M'),
		(NewID(), @madr, '244', '2', 'A', 2, 'M'), (NewID(), @madr, '245', '2', 'A', 2, 'M'), (NewID(), @madr, '246', '2', 'A', 2, 'M'), (NewID(), @madr, '247', '2', 'A', 2, 'M'), (NewID(), @madr, '248', '2', 'A', 2, 'M'),
		(NewID(), @madr, '249', '2', 'A', 2, 'M'), (NewID(), @madr, '250', '2', 'B', 2, 'M'), (NewID(), @madr, '260', '2', 'B', 2, 'M'), (NewID(), @madr, '262', '2', 'B', 2, 'M'), (NewID(), @madr, '264', '2', 'B', 2, 'M'),
		(NewID(), @madr, '266', '2', 'B', 2, 'M'), (NewID(), @madr, '267', '2', 'B', 2, 'M'), (NewID(), @madr, '268', '2', 'B', 2, 'M'), (NewID(), @madr, '269', '2', 'B', 2, 'M'), (NewID(), @madr, '270', '2', 'B', 1, 'M'),
		(NewID(), @madr, '272', '2', 'B', 2, 'M'), (NewID(), @madr, '274', '2', 'B', 2, 'M'), (NewID(), @madr, '275', '2', 'B', 2, 'M'), (NewID(), @madr, '276', '2', 'B', 2, 'M'), (NewID(), @madr, '277', '2', 'B', 2, 'M'),
		(NewID(), @madr, '278', '2', 'B', 2, 'M'), (NewID(), @madr, '279', '2', 'B', 2, 'M'), (NewID(), @madr, '280', '2', 'B', 2, 'M'), (NewID(), @madr, '281', '2', 'B', 2, 'M'), (NewID(), @madr, '282', '2', 'B', 2, 'M'),
		(NewID(), @madr, '284', '2', 'B', 2, 'M'), (NewID(), @madr, '322', '3', 'B', 2, 'F'), (NewID(), @madr, '328', '3', 'A', 2, 'F'), (NewID(), @madr, '330', '3', 'A', 2, 'F'), (NewID(), @madr, '331', '3', 'A', 2, 'F'),
		(NewID(), @madr, '332', '3', 'A', 2, 'F'), (NewID(), @madr, '333', '3', 'A', 2, ''), (NewID(), @madr, '334', '3', 'A', 2, 'F'), (NewID(), @madr, '335', '3', 'A', 2, 'F'), (NewID(), @madr, '336', '3', 'A', 2, 'F'),
		(NewID(), @madr, '337', '3', 'A', 2, 'F'), (NewID(), @madr, '338', '3', 'A', 2, 'F'), (NewID(), @madr, '339', '3', 'A', 2, 'F'), (NewID(), @madr, '340', '3', 'A', 1, 'F'), (NewID(), @madr, '341', '3', 'A', 2, 'F'),
		(NewID(), @madr, '342', '3', 'A', 2, 'F'), (NewID(), @madr, '343', '3', 'A', 2, 'F'), (NewID(), @madr, '344', '3', 'A', 2, 'F'), (NewID(), @madr, '345', '3', 'A', 2, 'F'), (NewID(), @madr, '346', '3', 'A', 2, 'F'),
		(NewID(), @madr, '347', '3', 'A', 2, 'F'), (NewID(), @madr, '348', '3', 'A', 2, 'F'), (NewID(), @madr, '349', '3', 'A', 2, 'F'), (NewID(), @madr, '350', '3', 'A', 2, 'F'), (NewID(), @madr, '360', '3', 'B', 2, 'F'),
		(NewID(), @madr, '362', '3', 'B', 2, 'F'), (NewID(), @madr, '364', '3', 'B', 2, 'F'), (NewID(), @madr, '366', '3', 'B', 2, 'F'), (NewID(), @madr, '367', '3', 'B', 2, 'F'), (NewID(), @madr, '368', '3', 'B', 2, 'F'),
		(NewID(), @madr, '369', '3', 'B', 2, 'F'), (NewID(), @madr, '370', '3', 'B', 2, 'F'), (NewID(), @madr, '372', '3', 'B', 1, 'F'), (NewID(), @madr, '374', '3', 'B', 2, 'F'), (NewID(), @madr, '375', '3', 'B', 2, 'F'),
		(NewID(), @madr, '376', '3', 'B', 2, 'F'), (NewID(), @madr, '377', '3', 'B', 2, 'F'), (NewID(), @madr, '378', '3', 'B', 2, 'F'), (NewID(), @madr, '379', '3', 'B', 2, 'F'), (NewID(), @madr, '380', '3', 'B', 2, 'F'),
		(NewID(), @madr, '381', '3', 'B', 2, 'F'), (NewID(), @madr, '382', '3', 'B', 2, 'F'), (NewID(), @madr, '384', '3', 'B', 2, 'F'), (NewID(), @madr, '422', '4', 'B', 2, 'M'), (NewID(), @madr, '428', '4', 'A', 2, 'F'),
		(NewID(), @madr, '430', '4', 'A', 2, 'F'), (NewID(), @madr, '431', '4', 'A', 2, 'F'), (NewID(), @madr, '432', '4', 'A', 2, 'F'), (NewID(), @madr, '433', '4', 'A', 2, 'F'), (NewID(), @madr, '434', '4', 'A', 2, 'F'),
		(NewID(), @madr, '435', '4', 'A', 2, 'F'), (NewID(), @madr, '436', '4', 'A', 2, 'F'), (NewID(), @madr, '437', '4', 'A', 2, 'F'), (NewID(), @madr, '438', '4', 'A', 2, 'F'), (NewID(), @madr, '439', '4', 'A', 2, 'F'),
		(NewID(), @madr, '440', '4', 'A', 1, 'F'), (NewID(), @madr, '441', '4', 'A', 2, 'F'), (NewID(), @madr, '442', '4', 'A', 2, 'F'), (NewID(), @madr, '443', '4', 'A', 2, 'F'), (NewID(), @madr, '444', '4', 'A', 2, 'F'),
		(NewID(), @madr, '445', '4', 'A', 2, 'F'), (NewID(), @madr, '446', '4', 'A', 2, 'F'), (NewID(), @madr, '447', '4', 'A', 2, 'F'), (NewID(), @madr, '448', '4', 'A', 2, 'F'), (NewID(), @madr, '449', '4', 'A', 1, 'M'),
		(NewID(), @madr, '450', '4', 'B', 2, 'M'), (NewID(), @madr, '460', '4', 'B', 2, 'M'), (NewID(), @madr, '462', '4', 'B', 2, 'M'), (NewID(), @madr, '464', '4', 'B', 2, 'M'), (NewID(), @madr, '466', '4', 'B', 2, 'M'),
		(NewID(), @madr, '467', '4', 'B', 2, 'M'), (NewID(), @madr, '468', '4', 'B', 2, 'M'), (NewID(), @madr, '469', '4', 'B', 2, 'M'), (NewID(), @madr, '470', '4', 'B', 1, 'M'), (NewID(), @madr, '472', '4', 'B', 2, 'M'),
		(NewID(), @madr, '474', '4', 'B', 2, 'M'), (NewID(), @madr, '475', '4', 'B', 2, 'M'), (NewID(), @madr, '476', '4', 'B', 2, 'M'), (NewID(), @madr, '477', '4', 'B', 2, 'M'), (NewID(), @madr, '478', '4', 'B', 2, 'M'),
		(NewID(), @madr, '479', '4', 'B', 2, 'M'), (NewID(), @madr, '480', '4', 'B', 2, 'M'), (NewID(), @madr, '481', '4', 'B', 2, 'M'), (NewID(), @madr, '482', '4', 'B', 2, 'M'), (NewID(), @madr, '484', '4', 'B', 2, 'M'),
		--Oaks 1
		(NewID(), @oak1, '101A', '1', '1', 1, ''), (NewID(), @oak1, '101B', '1', '1', 1, ''), (NewID(), @oak1, '102A', '1', '1', 1, ''), (NewID(), @oak1, '102B', '1', '1', 1, ''), (NewID(), @oak1, '103A', '1', '1', 1, ''),
		(NewID(), @oak1, '103B', '1', '1', 1, ''), (NewID(), @oak1, '111A', '1', '1', 1, ''), (NewID(), @oak1, '111B', '1', '1', 1, ''), (NewID(), @oak1, '112A', '1', '1', 1, ''), (NewID(), @oak1, '112B', '1', '1', 1, ''),
		(NewID(), @oak1, '113A', '1', '1', 1, ''), (NewID(), @oak1, '113B', '1', '1', 1, ''), (NewID(), @oak1, '114', '1', '1', 2, ''), (NewID(), @oak1, '200', '2', '2', 2, ''), (NewID(), @oak1, '201A', '2', '2', 1, ''),
		(NewID(), @oak1, '201B', '2', '2', 1, ''), (NewID(), @oak1, '202A', '2', '2', 1, ''), (NewID(), @oak1, '202B', '2', '2', 1, ''), (NewID(), @oak1, '203A', '2', '2', 1, ''), (NewID(), @oak1, '203B', '2', '2', 1, ''),
		(NewID(), @oak1, '211A', '2', '2', 1, ''), (NewID(), @oak1, '211B', '2', '2', 1, ''), (NewID(), @oak1, '212A', '2', '2', 1, ''), (NewID(), @oak1, '212B', '2', '2', 1, ''), (NewID(), @oak1, '213A', '2', '2', 1, ''),
		(NewID(), @oak1, '213B', '2', '2', 1, ''), (NewID(), @oak1, '214', '2', '2', 2, ''), (NewID(), @oak1, '300', '3', '3', 2, ''), (NewID(), @oak1, '301A', '3', '3', 1, ''), (NewID(), @oak1, '301B', '3', '3', 1, ''),
		(NewID(), @oak1, '302A', '3', '3', 1, ''), (NewID(), @oak1, '302B', '3', '3', 1, ''), (NewID(), @oak1, '303A', '3', '3', 1, ''), (NewID(), @oak1, '303B', '3', '3', 1, ''), (NewID(), @oak1, '311A', '3', '3', 1, ''),
		(NewID(), @oak1, '311B', '3', '3', 1, ''), (NewID(), @oak1, '312A', '3', '3', 1, ''), (NewID(), @oak1, '312B', '3', '3', 1, ''), (NewID(), @oak1, '313A', '3', '3', 1, ''), (NewID(), @oak1, '313B', '3', '3', 1, ''),
		(NewID(), @oak1, '314', '3', '3', 2, ''), (NewID(), @oak1, '400', '4', '4', 2, ''), (NewID(), @oak1, '401A', '4', '4', 1, ''), (NewID(), @oak1, '401B', '4', '4', 1, ''), (NewID(), @oak1, '402A', '4', '4', 1, ''),
		(NewID(), @oak1, '402B', '4', '4', 1, ''), (NewID(), @oak1, '403A', '4', '4', 1, ''), (NewID(), @oak1, '403B', '4', '4', 1, ''), (NewID(), @oak1, '411A', '4', '4', 1, ''), (NewID(), @oak1, '411B', '4', '4', 1, ''),
		(NewID(), @oak1, '412A', '4', '4', 1, ''), (NewID(), @oak1, '412B', '4', '4', 1, ''), (NewID(), @oak1, '413A', '4', '4', 1, ''), (NewID(), @oak1, '413B', '4', '4', 1, ''), (NewID(), @oak1, '414', '4', '4', 2, ''),
		(NewID(), @oak1, '500', '5', '5', 2, ''), (NewID(), @oak1, '501A', '5', '5', 1, ''), (NewID(), @oak1, '501B', '5', '5', 1, ''), (NewID(), @oak1, '502A', '5', '5', 1, ''), (NewID(), @oak1, '502B', '5', '5', 1, ''),
		(NewID(), @oak1, '503A', '5', '5', 1, ''), (NewID(), @oak1, '503B', '5', '5', 1, ''), (NewID(), @oak1, '511A', '5', '5', 1, ''), (NewID(), @oak1, '511B', '5', '5', 1, ''), (NewID(), @oak1, '512A', '5', '5', 1, ''),
		(NewID(), @oak1, '512B', '5', '5', 1, ''), (NewID(), @oak1, '513A', '5', '5', 1, ''), (NewID(), @oak1, '513B', '5', '5', 1, ''), (NewID(), @oak1, '514', '5', '5', 2, ''),
		--Oaks 2
		(NewID(), @oak2, '101A', '1', '1', 1, ''), (NewID(), @oak2, '101B', '1', '1', 1, ''), (NewID(), @oak2, '102A', '1', '1', 1, ''), (NewID(), @oak2, '102B', '1', '1', 1, ''), (NewID(), @oak2, '103A', '1', '1', 1, ''),
		(NewID(), @oak2, '103B', '1', '1', 1, ''), (NewID(), @oak2, '111A', '1', '1', 1, ''), (NewID(), @oak2, '111B', '1', '1', 1, ''), (NewID(), @oak2, '112A', '1', '1', 1, ''), (NewID(), @oak2, '112B', '1', '1', 1, ''),
		(NewID(), @oak2, '113A', '1', '1', 1, ''), (NewID(), @oak2, '113B', '1', '1', 1, ''), (NewID(), @oak2, '114', '1', '1', 2, ''), (NewID(), @oak2, '200', '2', '2', 2, ''), (NewID(), @oak2, '201A', '2', '2', 1, ''),
		(NewID(), @oak2, '201B', '2', '2', 1, ''), (NewID(), @oak2, '202A', '2', '2', 1, ''), (NewID(), @oak2, '202B', '2', '2', 1, ''), (NewID(), @oak2, '203A', '2', '2', 1, ''), (NewID(), @oak2, '203B', '2', '2', 1, ''),
		(NewID(), @oak2, '211A', '2', '2', 1, ''), (NewID(), @oak2, '211B', '2', '2', 1, ''), (NewID(), @oak2, '212A', '2', '2', 1, ''), (NewID(), @oak2, '212B', '2', '2', 1, ''), (NewID(), @oak2, '213A', '2', '2', 1, ''),
		(NewID(), @oak2, '213B', '2', '2', 1, ''), (NewID(), @oak2, '214', '2', '2', 2, ''), (NewID(), @oak2, '300', '3', '3', 2, ''), (NewID(), @oak2, '301A', '3', '3', 1, ''), (NewID(), @oak2, '301B', '3', '3', 1, ''),
		(NewID(), @oak2, '302A', '3', '3', 1, ''), (NewID(), @oak2, '302B', '3', '3', 1, ''), (NewID(), @oak2, '303A', '3', '3', 1, ''), (NewID(), @oak2, '303B', '3', '3', 1, ''), (NewID(), @oak2, '311A', '3', '3', 1, ''),
		(NewID(), @oak2, '311B', '3', '3', 1, ''), (NewID(), @oak2, '312A', '3', '3', 1, ''), (NewID(), @oak2, '312B', '3', '3', 1, ''), (NewID(), @oak2, '313A', '3', '3', 1, ''), (NewID(), @oak2, '313B', '3', '3', 1, ''),
		(NewID(), @oak2, '314', '3', '3', 2, ''), (NewID(), @oak2, '400', '4', '4', 2, ''), (NewID(), @oak2, '401A', '4', '4', 1, ''), (NewID(), @oak2, '401B', '4', '4', 1, ''), (NewID(), @oak2, '402A', '4', '4', 1, ''),
		(NewID(), @oak2, '402B', '4', '4', 1, ''), (NewID(), @oak2, '403A', '4', '4', 1, ''), (NewID(), @oak2, '403B', '4', '4', 1, ''), (NewID(), @oak2, '411A', '4', '4', 1, ''), (NewID(), @oak2, '411B', '4', '4', 1, ''),
		(NewID(), @oak2, '412A', '4', '4', 1, ''), (NewID(), @oak2, '412B', '4', '4', 1, ''), (NewID(), @oak2, '413A', '4', '4', 1, ''), (NewID(), @oak2, '413B', '4', '4', 1, ''), (NewID(), @oak2, '414', '4', '4', 2, ''),
		(NewID(), @oak2, '500', '5', '5', 2, ''), (NewID(), @oak2, '501A', '5', '5', 1, ''), (NewID(), @oak2, '501B', '5', '5', 1, ''), (NewID(), @oak2, '502A', '5', '5', 1, ''), (NewID(), @oak2, '502B', '5', '5', 1, ''),
		(NewID(), @oak2, '503A', '5', '5', 1, ''), (NewID(), @oak2, '503B', '5', '5', 1, ''), (NewID(), @oak2, '511A', '5', '5', 1, ''), (NewID(), @oak2, '511B', '5', '5', 1, ''), (NewID(), @oak2, '512A', '5', '5', 1, ''),
		(NewID(), @oak2, '512B', '5', '5', 1, ''), (NewID(), @oak2, '513A', '5', '5', 1, ''), (NewID(), @oak2, '513B', '5', '5', 1, ''), (NewID(), @oak2, '514', '5', '5', 2, ''),
		--Oaks 3
		(NewID(), @oak3, '102A', '1', '1', 1, ''), (NewID(), @oak3, '102B', '1', '1', 1, ''), (NewID(), @oak3, '103A', '1', '1', 1, ''), (NewID(), @oak3, '103B', '1', '1', 1, ''), (NewID(), @oak3, '111A', '1', '1', 1, ''),
		(NewID(), @oak3, '111B', '1', '1', 1, ''), (NewID(), @oak3, '112A', '1', '1', 1, ''), (NewID(), @oak3, '112B', '1', '1', 1, ''), (NewID(), @oak3, '113A', '1', '1', 1, ''), (NewID(), @oak3, '113B', '1', '1', 1, ''),
		(NewID(), @oak3, '114', '1', '1', 2, ''), (NewID(), @oak3, '200', '2', '2', 2, ''), (NewID(), @oak3, '201A', '2', '2', 1, ''), (NewID(), @oak3, '201B', '2', '2', 1, ''), (NewID(), @oak3, '202A', '2', '2', 1, ''),
		(NewID(), @oak3, '202B', '2', '2', 1, ''), (NewID(), @oak3, '203A', '2', '2', 1, ''), (NewID(), @oak3, '203B', '2', '2', 1, ''), (NewID(), @oak3, '211A', '2', '2', 1, ''), (NewID(), @oak3, '211B', '2', '2', 1, ''),
		(NewID(), @oak3, '212A', '2', '2', 1, ''), (NewID(), @oak3, '212B', '2', '2', 1, ''), (NewID(), @oak3, '213A', '2', '2', 1, ''), (NewID(), @oak3, '213B', '2', '2', 1, ''), (NewID(), @oak3, '214', '2', '2', 2, ''),
		(NewID(), @oak3, '300', '3', '3', 2, ''), (NewID(), @oak3, '301A', '3', '3', 1, ''), (NewID(), @oak3, '301B', '3', '3', 1, ''), (NewID(), @oak3, '302A', '3', '3', 1, ''), (NewID(), @oak3, '302B', '3', '3', 1, ''),
		(NewID(), @oak3, '303A', '3', '3', 1, ''), (NewID(), @oak3, '303B', '3', '3', 1, ''), (NewID(), @oak3, '311A', '3', '3', 1, ''), (NewID(), @oak3, '311B', '3', '3', 1, ''), (NewID(), @oak3, '312A', '3', '3', 1, ''),
		(NewID(), @oak3, '312B', '3', '3', 1, ''), (NewID(), @oak3, '313A', '3', '3', 1, ''), (NewID(), @oak3, '313B', '3', '3', 1, ''), (NewID(), @oak3, '314', '3', '3', 2, ''), (NewID(), @oak3, '400', '4', '4', 2, ''),
		(NewID(), @oak3, '401A', '4', '4', 1, ''), (NewID(), @oak3, '401B', '4', '4', 1, ''), (NewID(), @oak3, '402A', '4', '4', 1, ''), (NewID(), @oak3, '402B', '4', '4', 1, ''), (NewID(), @oak3, '403A', '4', '4', 1, ''),
		(NewID(), @oak3, '403B', '4', '4', 1, ''), (NewID(), @oak3, '411A', '4', '4', 1, ''), (NewID(), @oak3, '411B', '4', '4', 1, ''), (NewID(), @oak3, '412A', '4', '4', 1, ''), (NewID(), @oak3, '412B', '4', '4', 1, ''),
		(NewID(), @oak3, '413A', '4', '4', 1, ''), (NewID(), @oak3, '413B', '4', '4', 1, ''), (NewID(), @oak3, '414', '4', '4', 2, ''), (NewID(), @oak3, '500', '5', '5', 2, ''), (NewID(), @oak3, '501A', '5', '5', 1, ''),
		(NewID(), @oak3, '501B', '5', '5', 1, ''), (NewID(), @oak3, '502A', '5', '5', 1, ''), (NewID(), @oak3, '502B', '5', '5', 1, ''), (NewID(), @oak3, '503A', '5', '5', 1, ''), (NewID(), @oak3, '503B', '5', '5', 1, ''),
		(NewID(), @oak3, '511A', '5', '5', 1, ''), (NewID(), @oak3, '511B', '5', '5', 1, ''), (NewID(), @oak3, '512A', '5', '5', 1, ''), (NewID(), @oak3, '512B', '5', '5', 1, ''), (NewID(), @oak3, '513A', '5', '5', 1, ''),
		(NewID(), @oak3, '513B', '5', '5', 1, ''), (NewID(), @oak3, '514', '5', '5', 2, ''),
		--Oaks 4
		(NewID(), @oak4, '101A', '1', '1', 1, ''), (NewID(), @oak4, '101B', '1', '1', 1, ''), (NewID(), @oak4, '102A', '1', '1', 1, ''), (NewID(), @oak4, '102B', '1', '1', 1, ''), (NewID(), @oak4, '103A', '1', '1', 1, ''),
		(NewID(), @oak4, '103B', '1', '1', 1, ''), (NewID(), @oak4, '111A', '1', '1', 1, ''), (NewID(), @oak4, '111B', '1', '1', 1, ''), (NewID(), @oak4, '112A', '1', '1', 1, ''), (NewID(), @oak4, '112B', '1', '1', 1, ''),
		(NewID(), @oak4, '113A', '1', '1', 1, ''), (NewID(), @oak4, '113B', '1', '1', 1, ''), (NewID(), @oak4, '114', '1', '1', 2, ''), (NewID(), @oak4, '200', '2', '2', 2, ''), (NewID(), @oak4, '201A', '2', '2', 1, ''),
		(NewID(), @oak4, '201B', '2', '2', 1, ''), (NewID(), @oak4, '202A', '2', '2', 1, ''), (NewID(), @oak4, '202B', '2', '2', 1, ''), (NewID(), @oak4, '203A', '2', '2', 1, ''), (NewID(), @oak4, '203B', '2', '2', 1, ''),
		(NewID(), @oak4, '211A', '2', '2', 1, ''), (NewID(), @oak4, '211B', '2', '2', 1, ''), (NewID(), @oak4, '212A', '2', '2', 1, ''), (NewID(), @oak4, '212B', '2', '2', 1, ''), (NewID(), @oak4, '213A', '2', '2', 1, ''),
		(NewID(), @oak4, '213B', '2', '2', 1, ''), (NewID(), @oak4, '214', '2', '2', 2, ''), (NewID(), @oak4, '300', '3', '3', 2, ''), (NewID(), @oak4, '301A', '3', '3', 1, ''), (NewID(), @oak4, '301B', '3', '3', 1, ''),
		(NewID(), @oak4, '302A', '3', '3', 1, ''), (NewID(), @oak4, '302B', '3', '3', 1, ''), (NewID(), @oak4, '303A', '3', '3', 1, ''), (NewID(), @oak4, '303B', '3', '3', 1, ''), (NewID(), @oak4, '311A', '3', '3', 1, ''),
		(NewID(), @oak4, '311B', '3', '3', 1, ''), (NewID(), @oak4, '312A', '3', '3', 1, ''), (NewID(), @oak4, '312B', '3', '3', 1, ''), (NewID(), @oak4, '313A', '3', '3', 1, ''), (NewID(), @oak4, '313B', '3', '3', 1, ''),
		(NewID(), @oak4, '314', '3', '3', 2, ''), (NewID(), @oak4, '400', '4', '4', 2, ''), (NewID(), @oak4, '401A', '4', '4', 1, ''), (NewID(), @oak4, '401B', '4', '4', 1, ''), (NewID(), @oak4, '402A', '4', '4', 1, ''),
		(NewID(), @oak4, '402B', '4', '4', 1, ''), (NewID(), @oak4, '403A', '4', '4', 1, ''), (NewID(), @oak4, '403B', '4', '4', 1, ''), (NewID(), @oak4, '411A', '4', '4', 1, ''), (NewID(), @oak4, '411B', '4', '4', 1, ''),
		(NewID(), @oak4, '412A', '4', '4', 1, ''), (NewID(), @oak4, '412B', '4', '4', 1, ''), (NewID(), @oak4, '413A', '4', '4', 1, ''), (NewID(), @oak4, '413B', '4', '4', 1, ''), (NewID(), @oak4, '414', '4', '4', 2, ''),
		(NewID(), @oak4, '500', '5', '5', 2, ''), (NewID(), @oak4, '501A', '5', '5', 1, ''), (NewID(), @oak4, '501B', '5', '5', 1, ''), (NewID(), @oak4, '502A', '5', '5', 1, ''), (NewID(), @oak4, '502B', '5', '5', 1, ''),
		(NewID(), @oak4, '503A', '5', '5', 1, ''), (NewID(), @oak4, '503B', '5', '5', 1, ''), (NewID(), @oak4, '511A', '5', '5', 1, ''), (NewID(), @oak4, '511B', '5', '5', 1, ''), (NewID(), @oak4, '512A', '5', '5', 1, ''),
		(NewID(), @oak4, '512B', '5', '5', 1, ''), (NewID(), @oak4, '513A', '5', '5', 1, ''), (NewID(), @oak4, '513B', '5', '5', 1, ''), (NewID(), @oak4, '514', '5', '5', 2, ''),
		--Oaks 5
		(NewID(), @oak5, '101A', '1', '1', 1, ''), (NewID(), @oak5, '101B', '1', '1', 1, ''), (NewID(), @oak5, '102A', '1', '1', 1, ''), (NewID(), @oak5, '102B', '1', '1', 1, ''), (NewID(), @oak5, '103A', '1', '1', 1, ''),
		(NewID(), @oak5, '103B', '1', '1', 1, ''), (NewID(), @oak5, '111A', '1', '1', 1, ''), (NewID(), @oak5, '111B', '1', '1', 1, ''), (NewID(), @oak5, '112A', '1', '1', 1, ''), (NewID(), @oak5, '112B', '1', '1', 1, ''),
		(NewID(), @oak5, '113A', '1', '1', 1, ''), (NewID(), @oak5, '113B', '1', '1', 1, ''), (NewID(), @oak5, '114', '1', '1', 2, ''), (NewID(), @oak5, '200', '2', '2', 2, ''), (NewID(), @oak5, '201A', '2', '2', 1, ''),
		(NewID(), @oak5, '201B', '2', '2', 1, ''), (NewID(), @oak5, '202A', '2', '2', 1, ''), (NewID(), @oak5, '202B', '2', '2', 1, ''), (NewID(), @oak5, '203A', '2', '2', 1, ''), (NewID(), @oak5, '203B', '2', '2', 1, ''),
		(NewID(), @oak5, '211A', '2', '2', 1, ''), (NewID(), @oak5, '211B', '2', '2', 1, ''), (NewID(), @oak5, '212A', '2', '2', 1, ''), (NewID(), @oak5, '212B', '2', '2', 1, ''), (NewID(), @oak5, '213A', '2', '2', 1, ''),
		(NewID(), @oak5, '213B', '2', '2', 1, ''), (NewID(), @oak5, '214', '2', '2', 2, ''), (NewID(), @oak5, '300', '3', '3', 2, ''), (NewID(), @oak5, '301A', '3', '3', 1, ''), (NewID(), @oak5, '301B', '3', '3', 1, ''),
		(NewID(), @oak5, '302A', '3', '3', 1, ''), (NewID(), @oak5, '302B', '3', '3', 1, ''), (NewID(), @oak5, '303A', '3', '3', 1, ''), (NewID(), @oak5, '303B', '3', '3', 1, ''), (NewID(), @oak5, '311A', '3', '3', 1, ''),
		(NewID(), @oak5, '311B', '3', '3', 1, ''), (NewID(), @oak5, '312A', '3', '3', 1, ''), (NewID(), @oak5, '312B', '3', '3', 1, ''), (NewID(), @oak5, '313A', '3', '3', 1, ''), (NewID(), @oak5, '313B', '3', '3', 1, ''),
		(NewID(), @oak5, '314', '3', '3', 2, ''), (NewID(), @oak5, '400', '4', '4', 2, ''), (NewID(), @oak5, '401A', '4', '4', 1, ''), (NewID(), @oak5, '401B', '4', '4', 1, ''), (NewID(), @oak5, '402A', '4', '4', 1, ''),
		(NewID(), @oak5, '402B', '4', '4', 1, ''), (NewID(), @oak5, '403A', '4', '4', 1, ''), (NewID(), @oak5, '403B', '4', '4', 1, ''), (NewID(), @oak5, '411A', '4', '4', 1, ''), (NewID(), @oak5, '411B', '4', '4', 1, ''),
		(NewID(), @oak5, '412A', '4', '4', 1, ''), (NewID(), @oak5, '412B', '4', '4', 1, ''), (NewID(), @oak5, '413A', '4', '4', 1, ''), (NewID(), @oak5, '413B', '4', '4', 1, ''), (NewID(), @oak5, '414', '4', '4', 2, ''),
		(NewID(), @oak5, '500', '5', '5', 2, ''), (NewID(), @oak5, '501A', '5', '5', 1, ''), (NewID(), @oak5, '501B', '5', '5', 1, ''), (NewID(), @oak5, '502A', '5', '5', 1, ''), (NewID(), @oak5, '502B', '5', '5', 1, ''),
		(NewID(), @oak5, '503A', '5', '5', 1, ''), (NewID(), @oak5, '503B', '5', '5', 1, ''), (NewID(), @oak5, '511A', '5', '5', 1, ''), (NewID(), @oak5, '511B', '5', '5', 1, ''), (NewID(), @oak5, '512A', '5', '5', 1, ''),
		(NewID(), @oak5, '512B', '5', '5', 1, ''), (NewID(), @oak5, '513A', '5', '5', 1, ''), (NewID(), @oak5, '513B', '5', '5', 1, ''), (NewID(), @oak5, '514', '5', '5', 2, ''),
		--Oaks 6
		(NewID(), @oak6, '101A', '1', '1', 1, ''), (NewID(), @oak6, '101B', '1', '1', 1, ''), (NewID(), @oak6, '102A', '1', '1', 1, ''), (NewID(), @oak6, '102B', '1', '1', 1, ''), (NewID(), @oak6, '103A', '1', '1', 1, ''),
		(NewID(), @oak6, '103B', '1', '1', 1, ''), (NewID(), @oak6, '111A', '1', '1', 1, ''), (NewID(), @oak6, '111B', '1', '1', 1, ''), (NewID(), @oak6, '112A', '1', '1', 1, ''), (NewID(), @oak6, '112B', '1', '1', 1, ''),
		(NewID(), @oak6, '113A', '1', '1', 1, ''), (NewID(), @oak6, '113B', '1', '1', 1, ''), (NewID(), @oak6, '114', '1', '1', 1, ''), (NewID(), @oak6, '200', '2', '2', 2, ''), (NewID(), @oak6, '201A', '2', '2', 1, ''),
		(NewID(), @oak6, '201B', '2', '2', 1, ''), (NewID(), @oak6, '202A', '2', '2', 1, ''), (NewID(), @oak6, '202B', '2', '2', 1, ''), (NewID(), @oak6, '203A', '2', '2', 1, ''), (NewID(), @oak6, '203B', '2', '2', 1, ''),
		(NewID(), @oak6, '211A', '2', '2', 1, ''), (NewID(), @oak6, '211B', '2', '2', 1, ''), (NewID(), @oak6, '212A', '2', '2', 1, ''), (NewID(), @oak6, '212B', '2', '2', 1, ''), (NewID(), @oak6, '213A', '2', '2', 1, ''),
		(NewID(), @oak6, '213B', '2', '2', 1, ''), (NewID(), @oak6, '214', '2', '2', 2, ''), (NewID(), @oak6, '300', '3', '3', 2, ''), (NewID(), @oak6, '301A', '3', '3', 1, ''), (NewID(), @oak6, '301B', '3', '3', 1, ''),
		(NewID(), @oak6, '302A', '3', '3', 1, ''), (NewID(), @oak6, '302B', '3', '3', 1, ''), (NewID(), @oak6, '303A', '3', '3', 1, ''), (NewID(), @oak6, '303B', '3', '3', 1, ''), (NewID(), @oak6, '311A', '3', '3', 1, ''),
		(NewID(), @oak6, '311B', '3', '3', 1, ''), (NewID(), @oak6, '312A', '3', '3', 1, ''), (NewID(), @oak6, '312B', '3', '3', 1, ''), (NewID(), @oak6, '313A', '3', '3', 1, ''), (NewID(), @oak6, '313B', '3', '3', 1, ''),
		(NewID(), @oak6, '314', '3', '3', 2, ''), (NewID(), @oak6, '400', '4', '4', 2, ''), (NewID(), @oak6, '401A', '4', '4', 1, ''), (NewID(), @oak6, '401B', '4', '4', 1, ''), (NewID(), @oak6, '402A', '4', '4', 1, ''),
		(NewID(), @oak6, '402B', '4', '4', 1, ''), (NewID(), @oak6, '403A', '4', '4', 1, ''), (NewID(), @oak6, '403B', '4', '4', 1, ''), (NewID(), @oak6, '411A', '4', '4', 1, ''), (NewID(), @oak6, '411B', '4', '4', 1, ''),
		(NewID(), @oak6, '412A', '4', '4', 1, ''), (NewID(), @oak6, '412B', '4', '4', 1, ''), (NewID(), @oak6, '413A', '4', '4', 1, ''), (NewID(), @oak6, '413B', '4', '4', 1, ''), (NewID(), @oak6, '414', '4', '4', 2, ''),
		(NewID(), @oak6, '500', '5', '5', 2, ''), (NewID(), @oak6, '501A', '5', '5', 1, ''), (NewID(), @oak6, '501B', '5', '5', 1, ''), (NewID(), @oak6, '502A', '5', '5', 1, ''), (NewID(), @oak6, '502B', '5', '5', 1, ''),
		(NewID(), @oak6, '503A', '5', '5', 1, ''), (NewID(), @oak6, '503B', '5', '5', 1, ''), (NewID(), @oak6, '511A', '5', '5', 1, ''), (NewID(), @oak6, '511B', '5', '5', 1, ''), (NewID(), @oak6, '512A', '5', '5', 1, ''),
		(NewID(), @oak6, '512B', '5', '5', 1, ''), (NewID(), @oak6, '513A', '5', '5', 1, ''), (NewID(), @oak6, '513B', '5', '5', 1, ''), (NewID(), @oak6, '514', '5', '5', 2, '');
		
	INSERT INTO dbo.CUS_Housing_Room (RoomID, BuildingID, RoomNumber, [Floor], Wing, Capacity, DefaultGender)
	VALUES
		--Swenson
		(NewID(), @swe , '001', '1', '1', 2, 'M'), (NewID(), @swe , '002', '1', '1', 2, 'M'), (NewID(), @swe , '003', '1', '1', 2, 'M'), (NewID(), @swe , '004', '1', '1', 2, 'M'), (NewID(), @swe , '005', '1', '1', 2, 'M'),
		(NewID(), @swe , '006', '1', '1', 2, 'M'), (NewID(), @swe , '007', '1', '1', 2, 'M'), (NewID(), @swe , '008', '1', '1', 2, 'M'), (NewID(), @swe , '009', '1', '1', 2, 'M'), (NewID(), @swe , '010', '1', '1', 2, 'M'),
		(NewID(), @swe , '011', '1', '1', 2, 'M'), (NewID(), @swe , '012', '1', '1', 2, 'M'), (NewID(), @swe , '013', '1', '1', 2, 'M'), (NewID(), @swe , '015', '1', '1', 1, 'M'),
		--Tarble
		(NewID(), @tar , '004', 'S', 'L', 3, 'F'), (NewID(), @tar , '005', 'S', 'L', 3, 'F'), (NewID(), @tar , '006', 'S', 'L', 3, 'F'), (NewID(), @tar , '007', 'S', 'L', 2, 'F'), (NewID(), @tar , '008', 'S', 'L', 3, 'F'),
		(NewID(), @tar , '009', 'S', 'L', 3, 'F'), (NewID(), @tar , '010', 'S', 'L', 3, 'F'), (NewID(), @tar , '011', 'S', 'L', 3, 'F'), (NewID(), @tar , '012', 'S', 'L', 4, 'F'), (NewID(), @tar , '014', 'T', 'S', 2, 'F'),
		(NewID(), @tar , '015', 'T', 'S', 2, 'F'), (NewID(), @tar , '016', 'T', 'S', 2, 'F'), (NewID(), @tar , '017', 'T', 'S', 2, 'F'), (NewID(), @tar , '018', 'T', 'S', 2, 'F'), (NewID(), @tar , '100', '1', 'L', 2, 'F'),
		(NewID(), @tar , '101', '1', 'L', 2, 'F'), (NewID(), @tar , '102', '1', 'L', 2, 'F'), (NewID(), @tar , '103', '1', 'L', 2, 'F'), (NewID(), @tar , '104', '1', 'L', 2, 'F'), (NewID(), @tar , '105', '1', 'L', 2, 'F'),
		(NewID(), @tar , '106', '1', 'L', 2, 'F'), (NewID(), @tar , '107', '1', 'L', 2, 'F'), (NewID(), @tar , '108', '1', 'L', 2, 'F'), (NewID(), @tar , '109', '1', 'L', 2, 'F'), (NewID(), @tar , '110', '1', 'L', 1, 'F'),
		(NewID(), @tar , '111', '1', 'L', 2, 'F'), (NewID(), @tar , '112', '1', 'L', 2, 'F'), (NewID(), @tar , '113', '1', 'L', 2, 'F'), (NewID(), @tar , '114', '1', 'L', 2, 'F'), (NewID(), @tar , '115', '1', 'L', 2, 'F'),
		(NewID(), @tar , '116', '1', 'L', 2, 'F'), (NewID(), @tar , '118', '1', 'L', 2, 'F'), (NewID(), @tar , '120', '1', 'L', 1, 'F'), (NewID(), @tar , '150', '1', 'S', 2, 'F'), (NewID(), @tar , '151', '1', 'S', 2, 'F'),
		(NewID(), @tar , '152', '1', 'S', 2, 'F'), (NewID(), @tar , '153', '1', 'S', 2, 'F'), (NewID(), @tar , '154', '1', 'S', 2, 'F'), (NewID(), @tar , '155', '1', 'S', 2, 'F'), (NewID(), @tar , '156', '1', 'S', 2, 'F'),
		(NewID(), @tar , '158', '1', 'S', 2, 'F'), (NewID(), @tar , '200', '2', 'L', 2, 'F'), (NewID(), @tar , '201', '2', 'L', 2, 'F'), (NewID(), @tar , '202', '2', 'L', 2, 'F'), (NewID(), @tar , '203', '2', 'L', 2, 'F'),
		(NewID(), @tar , '204', '2', 'L', 2, 'F'), (NewID(), @tar , '205', '2', 'L', 2, 'F'), (NewID(), @tar , '206', '2', 'L', 2, 'F'), (NewID(), @tar , '207', '2', 'L', 2, 'F'), (NewID(), @tar , '208', '2', 'L', 2, 'F'),
		(NewID(), @tar , '209', '2', 'L', 2, 'F'), (NewID(), @tar , '210', '2', 'L', 2, 'F'), (NewID(), @tar , '211', '2', 'L', 2, 'F'), (NewID(), @tar , '212', '2', 'L', 1, 'F'), (NewID(), @tar , '213', '2', 'L', 2, 'F'),
		(NewID(), @tar , '214', '2', 'L', 2, 'F'), (NewID(), @tar , '215', '2', 'L', 2, 'F'), (NewID(), @tar , '216', '2', 'L', 2, 'F'), (NewID(), @tar , '218', '2', 'L', 2, 'F'), (NewID(), @tar , '220', '2', 'L', 1, 'F'),
		(NewID(), @tar , '251', '2', 'S', 2, 'F'), (NewID(), @tar , '252', '2', 'S', 1, 'F'), (NewID(), @tar , '253', '2', 'S', 2, 'F'), (NewID(), @tar , '254', '2', 'S', 2, 'F'), (NewID(), @tar , '255', '2', 'S', 2, 'F'),
		(NewID(), @tar , '256', '2', 'S', 2, 'F'), (NewID(), @tar , '258', '2', 'S', 2, 'F'), (NewID(), @tar , '260', '2', 'S', 2, 'F'), (NewID(), @tar , '300', '3', 'L', 2, 'F'), (NewID(), @tar , '301', '3', 'L', 1, 'F'),
		(NewID(), @tar , '302', '3', 'L', 2, 'F'), (NewID(), @tar , '303', '3', 'L', 2, 'F'), (NewID(), @tar , '304', '3', 'L', 2, 'F'), (NewID(), @tar , '305', '3', 'L', 2, 'F'), (NewID(), @tar , '306', '3', 'L', 2, 'F'),
		(NewID(), @tar , '307', '3', 'L', 2, 'F'), (NewID(), @tar , '308', '3', 'L', 1, 'F'), (NewID(), @tar , '309', '3', 'L', 2, 'F'), (NewID(), @tar , '310', '3', 'L', 2, 'F'), (NewID(), @tar , '311', '3', 'L', 2, 'F'),
		(NewID(), @tar , '312', '3', 'L', 2, 'F'), (NewID(), @tar , '313', '3', 'L', 2, 'F'), (NewID(), @tar , '314', '3', 'L', 2, 'F'), (NewID(), @tar , '315', '3', 'L', 2, 'F'), (NewID(), @tar , '316', '3', 'L', 2, 'F'),
		(NewID(), @tar , '318', '3', 'L', 2, 'F'), (NewID(), @tar , '320', '3', 'L', 1, 'F'), (NewID(), @tar , '350', '3', 'S', 1, 'F'), (NewID(), @tar , '351', '3', 'S', 2, 'F'), (NewID(), @tar , '352', '3', 'S', 2, 'F'),
		(NewID(), @tar , '353', '3', 'S', 2, 'F'), (NewID(), @tar , '354', '3', 'S', 2, 'F'), (NewID(), @tar , '355', '3', 'S', 1, 'F'), (NewID(), @tar , '356', '3', 'S', 2, 'F'), (NewID(), @tar , '358', '3', 'S', 2, 'F'),
		(NewID(), @tar , '360', '3', 'S', 2, 'F'), (NewID(), @tar , '400', '4', 'L', 2, 'F'), (NewID(), @tar , '401', '4', 'L', 2, 'F'), (NewID(), @tar , '402', '4', 'L', 2, 'F'), (NewID(), @tar , '403', '4', 'L', 2, 'F'),
		(NewID(), @tar , '404', '4', 'L', 2, 'F'), (NewID(), @tar , '405', '4', 'L', 2, 'F'), (NewID(), @tar , '406', '4', 'L', 2, 'F'), (NewID(), @tar , '407', '4', 'L', 2, 'F'), (NewID(), @tar , '408', '4', 'L', 2, 'F'),
		(NewID(), @tar , '409', '4', 'L', 2, 'F'), (NewID(), @tar , '410', '4', 'L', 1, 'F'), (NewID(), @tar , '411', '4', 'L', 2, 'F'), (NewID(), @tar , '412', '4', 'L', 2, 'F'), (NewID(), @tar , '413', '4', 'L', 2, 'F'),
		(NewID(), @tar , '414', '4', 'L', 2, 'F'), (NewID(), @tar , '415', '4', 'L', 2, 'F'), (NewID(), @tar , '416', '4', 'L', 2, 'F'), (NewID(), @tar , '418', '4', 'L', 2, 'F'), (NewID(), @tar , '420', '4', 'L', 1, 'F'),
		(NewID(), @tar , '450', '4', 'S', 1, 'F'), (NewID(), @tar , '451', '4', 'S', 2, 'F'), (NewID(), @tar , '452', '4', 'S', 2, 'F'), (NewID(), @tar , '453', '4', 'S', 2, 'F'), (NewID(), @tar , '454', '4', 'S', 2, 'F'),
		(NewID(), @tar , '455', '4', 'S', 2, 'F'), (NewID(), @tar , '456', '4', 'S', 2, 'F'), (NewID(), @tar , '458', '4', 'S', 2, 'F'), (NewID(), @tar , '460', '4', 'S', 2, 'F');

	DECLARE	@createdRooms	INT	=	(SELECT COUNT(*) FROM CUS_Housing_Room);
	PRINT 'Created ' + CAST(@createdRooms AS VARCHAR) + ' rooms';

	--IMPORTANT!!: All Greek default values came from 2014 housing data. Changes made via the administrative screens are not reflected in this SQL file.

	--Beta Phi Epsilon (DEN: 300, 304, 306)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Beta Phi Epsilon')
	WHERE
		BuildingID		=	@den
	AND
		RoomNumber		IN	('300','304','306');
	PRINT 'Updated Beta Phi Epsilon beds';

	--Tau Kappa Epsilon (DEN: 349, 358, 359, 362, 368)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Tau Kappa Epsilon')
	WHERE
		BuildingID		=	@den
	AND
		RoomNumber		IN	('349','358','359','362','368');
	PRINT 'Updated Tau Kappa Epsilon beds';

	--Chi Omega (DEN: 400, 401, 402, 403, 404, 405, 409, 410, 411, 412, 413, 414, 416, 418, 420)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Chi Omega')
	WHERE
		BuildingID		=	@den
	AND
		RoomNumber		IN	('400','401','402','403','404','405','409','410','411','412','413','414','416','418','420');
	PRINT 'Updated Chi Omega beds';

	--Sigma Omega Sigma (JOH: 202, 206, 216)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Sigma Omega Sigma')
	WHERE
		BuildingID		=	@joh
	AND
		RoomNumber		IN	('202','206','216');
	PRINT 'Updated Sigma Omega Sigma beds';

	--Delta Upsilon (JOH: 300,302,304,305,306,307,310,311,312,313,314,320,322)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Delta Upsilon')
	WHERE
		BuildingID		=	@joh
	AND
		RoomNumber		IN	('300','302','304','305','306','307','310','311','312','313','314','320','322');
	PRINT 'Updated Delta Upsilon beds';

	--Phi Kappa Sigma (JOH: 401,403,404,412,413,414,415,422)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Phi Kappa Sigma')
	WHERE
		BuildingID		=	@joh
	AND
		RoomNumber		IN	('401','403','404','412','413','414','415','422');
	PRINT 'Updated Phi Kappa Sigma beds';

	--Alpha Chi Omega (JOH: 450,453,454,457,461,462,463,464,465,468)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Alpha Chi Omega')
	WHERE
		BuildingID		=	@joh
	AND
		RoomNumber		IN	('450','453','454','457','461','462','463','464','465','468');
	PRINT 'Updated Alpha Chi Omega beds';

	--Tau Sigma Chi (MADR: 128,130,132,134,136,138,139,141,142,144,145,146,147,149,165,167,168,169,170,172,176,178,180)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Tau Sigma Chi')
	WHERE
		BuildingID		=	@madr
	AND
		RoomNumber		IN	('128','130','132','134','136','138','139','141','142','144','145','146','147','149','165','167','168','169','170','172','176','178','180');
	PRINT 'Updated Tau Sigma Chi beds';

	--Tau Sigma Phi (MADR: 222,264,274)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Tau Sigma Phi')
	WHERE
		BuildingID		=	@madr
	AND
		RoomNumber		IN	('222','264','274');
	PRINT 'Updated Tau Sigma Phi beds';

	--Kappa Phi Eta (MADR: 328,334,336,337,339,341,349)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Kappa Phi Eta')
	WHERE
		BuildingID		=	@madr
	AND
		RoomNumber		IN	('328','334','336','337','339','341','349');
	PRINT 'Updated Kappa Phi Eta beds';

	--Delta Omega Nu (MADR: 422,481)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Delta Omega Nu')
	WHERE
		BuildingID		=	@madr
	AND
		RoomNumber		IN	('422','481');
	PRINT 'Updated Delta Omega Nu beds';

	--Sigma Alpha Chi (TAR: 202,204)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Sigma Alpha Chi')
	WHERE
		BuildingID		=	@tar
	AND
		RoomNumber		IN	('202','204');
	PRINT 'Updated Sigma Alpha Chi beds';

	--Pi Theta (TAR: 306,310,312,314,316,318,320)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Pi Theta')
	WHERE
		BuildingID		=	@tar
	AND
		RoomNumber		IN	('306','310','312','314','316','318','320');
	PRINT 'Updated Pi Theta beds';

	--Delta Omega Epsilon (Tarble 1 Long)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Delta Omega Epsilon')
	WHERE
		BuildingID		=	@tar
	AND
		[Floor]			=	'1'
	AND
		Wing			=	'L'
	PRINT 'Updated Delta Omega Epsilon beds';

	--Zeta Tau Sigma (Denhart 1B)
	UPDATE
		CUS_Housing_Room
	SET
		DefaultGreek	=	(SELECT greekid FROM CUS_HousingSelectionGreek WHERE greekname = 'Zeta Tau Sigma')
	WHERE
		BuildingID		=	@den
	AND
		[Floor]			=	'1'
	AND
		Wing			=	'B'
	PRINT 'Updated Zeta Tau Sigma beds';

	--RA Rooms
	UPDATE
		CUS_Housing_Room
	SET
		IsRA	=	1
	WHERE
		--Apartments
		(
			BuildingID	=	@apt
			AND
			RoomNumber	IN	('001')
		)
		OR
		--Denhart
		(
			BuildingID	=	@den
			AND
			RoomNumber	IN	('012','020','108','151','212','251','308','320','351','408','451')
		)
		OR
		--Johnson
		(
			BuildingID	=	@joh
			AND
			RoomNumber	IN	('008','110','158','208','258','308','351','358','410','460')
		)
		OR
		--Madrigrano
		(
			BuildingID	=	@madr
			AND
			RoomNumber	IN	('140','174','240','270','340','372','440','449','470')
		)
		OR
		--Oaks 1
		(
			BuildingID	=	@oak1
			AND
			RoomNumber	IN	('211A','211B','403A','403B')
		)
		OR
		--Oaks 2
		(
			BuildingID	=	@oak2
			AND
			RoomNumber	IN	('211A','211B','403A','403B')
		)
		OR
		--Oaks 3
		(
			BuildingID	=	@oak3
			AND
			RoomNumber	IN	('211A','211B','403A','403B')
		)
		OR
		--Oaks 4
		(
			BuildingID	=	@oak4
			AND
			RoomNumber	IN	('211A','211B','403A','403B')
		)
		OR
		--Oaks 5
		(
			BuildingID	=	@oak5
			AND
			RoomNumber	IN	('211A','211B','403A','403B')
		)
		OR
		--Oaks 6
		(
			BuildingID	=	@oak6
			AND
			RoomNumber	IN	('114','211A','211B','403A','403B')
		)
		OR
		--Swenson
		(
			BuildingID	=	@swe
			AND
			RoomNumber	IN	('015')
		)
		OR
		--Tarble
		(
			BuildingID	=	@tar
			AND
			RoomNumber	IN	('007','110','212','252','301','308','355','410')
		)
	PRINT 'Updated RA beds';
ROLLBACK