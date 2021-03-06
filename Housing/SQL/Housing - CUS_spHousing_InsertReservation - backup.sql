USE [ICS_NET_TEST]
GO
/****** Object:  StoredProcedure [dbo].[CUS_spHousing_insertReservation]    Script Date: 3/28/2019 9:08:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
	ALTER PROCEDURE [dbo].[CUS_spHousing_insertReservation]
		@guidRoomSession	UNIQUEIDENTIFIER,
		@guidInvitee		UNIQUEIDENTIFIER,
		@guidInviter		UNIQUEIDENTIFIER
	AS
		DECLARE @guidNewRoomSessionID	UNIQUEIDENTIFIER,
				@intCapacity			INT;
		SELECT
			@guidNewRoomSessionID	=	HRS.RoomSessionID, @intCapacity = HR.Capacity
		FROM
			CUS_Housing_Room	HR	INNER JOIN	CUS_Housing_RoomSession	HRS		ON	HR.RoomID	=	HRS.RoomID
									INNER JOIN	(
													SELECT
														SUBSTRING(HRsub.RoomNumber, 1, 3) AS RoomNumberOnly, HRsub.BuildingID
													FROM
														CUS_Housing_Room	HRsub	INNER JOIN	CUS_Housing_RoomSession	HRSsub	ON	HRsub.RoomID		=	HRSsub.RoomID
																																AND	HRSsub.HousingYear	=	YEAR(GETDATE())
													WHERE
														HRSsub.RoomSessionID	=	@guidRoomSession
												) ROS							ON	SUBSTRING(HR.RoomNumber, 1, 3)	=	ROS.RoomNumberOnly
																				AND	HR.BuildingID					=	ROS.BuildingID
									LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID				=	HRStu.RoomSessionID
		WHERE
			HRS.HousingYear = YEAR(GETDATE())
		GROUP BY
			HRS.RoomSessionID, HR.Capacity
		HAVING
			HR.Capacity > COUNT(HRStu.RoomStudentID)

        INSERT INTO CUS_Housing_RoomReservation (RoomSessionID, StudentID, ReservationTime, CreatedByID)
        VALUES(@guidNewRoomSessionID, @guidInvitee, GETDATE(), @guidInviter)

		SELECT @guidNewRoomSessionID AS 'RoomSessionID';
