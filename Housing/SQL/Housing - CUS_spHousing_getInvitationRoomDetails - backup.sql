USE [ICS_NET_TEST]
GO
/****** Object:  StoredProcedure [dbo].[CUS_spHousing_getInvitationRoomDetails]    Script Date: 3/28/2019 9:12:29 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
	ALTER PROCEDURE [dbo].[CUS_spHousing_getInvitationRoomDetails]
		@guidRoomSessionID	UNIQUEIDENTIFIER
	AS
        SELECT
            HB.BuildingName, HB.BuildingCode, SUBSTRING(HR.RoomNumber, 1, 3) AS RoomNumberOnly, SUM(HR.Capacity) AS Capacity
        FROM
            CUS_Housing_Room	HR	INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
										,
										(
											SELECT
												BuildingCode, SUBSTRING(RoomNumber, 1, 3) AS RoomNumberOnly
											FROM
												CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HRsub	ON	HRS.RoomID			=	HRsub.RoomID
																			INNER JOIN	CUS_Housing_Building	HBsub	ON	HRsub.BuildingID	=	HBsub.BuildingID
											WHERE
												HRS.RoomSessionID	=	@guidRoomSessionID
										)								Sess
		WHERE
			SUBSTRING(HR.RoomNumber, 1, 3)	=	Sess.RoomNumberOnly
		AND
			HB.BuildingCode					=	Sess.BuildingCode
		GROUP BY
			HB.BuildingName, HB.BuildingCode, SUBSTRING(HR.RoomNumber, 1, 3)
		ORDER BY
			HB.BuildingName, RoomNumberOnly
