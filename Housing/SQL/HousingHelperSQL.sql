use ICS_NET
SELECT * FROM CUS_HousingAssignmentSurvey WHERE matched like '2013RA'

SELECT CUS_HousingSelectionInvitations.*, 'invited' as status
FROM CUS_HousingSelectionInvitations
WHERE yr = 2013
AND sess = 'RA'

select * from FWK_User where HostID in (1253919,1323503,1325243)

select FWK_User.*, 'u=' + SUBSTRING(HostID, PATINDEX('%[^0]%', HostID+'.'), LEN(HostID)) + '&v=' + CAST(ID AS NVARCHAR(36)) from FWK_User where HostID = 1282253 --IN (1105140,1104155,771877,1105400,1362129,1100834,794718,514394)

select * from CUS_HousingSelectionStartdate
update CUS_HousingSelectionStartdate set startdate = '2014-04-22' where id = 1
update CUS_HousingSelectionStartdate set startdate = '2014-04-23' where id = 2

SELECT CAST(HostID AS INT) AS HostID
FROM FWK_User
WHERE ID = '0E2D510B-DDBD-4288-BDC5-209D81B1A320'

u=1257834&v=0E2D510B-DDBD-4288-BDC5-209D81B1A320
u=766640&v=FFB71AE4-7BEB-436B-A64B-3DBAFDADE70A
u=1225014&v=B2F6ED98-C9BE-440E-BA81-1F25327CE62C

SELECT TOP 20 *
FROM ICS_ContentImage

select * from ICS_UploadFile where ID = 'E3EE6E77-0E20-4448-8CB8-E10864554ACB'




SELECT Firstname, Lastname, HostID*1 AS ID, PayPeriodID, CAST(StartTime AS DATE) AS Day, StartTime, EndTime, EndTime - StartTime AS TimeWorked
FROM FWK_User	INNER JOIN	STF_TimecardEntry	ON	FWK_User.ID							=		STF_TimecardEntry.EmployeeUserID
				INNER JOIN	STF_TimecardHours	ON	STF_TimecardEntry.TimecardEntryID	=		STF_TimecardHours.TimecardEntryID
												AND	STF_TimecardHours.StartTime			IS NOT	NULL
												AND	STF_TimecardHours.EndTime			IS NOT	NULL
/*
WHERE FWK_User.ID = STF_TimecardEntry.EmployeeUserID
AND STF_TimecardEntry.TimecardEntryID = STF_TimecardHours.TimecardEntryID
AND STF_TimecardHours.StartTime IS NOT NULL
AND STF_TimecardHours.EndTime IS NOT NULL
*/

SELECT student.Firstname, student.Lastname, student.HostID*1 AS ID, logged.PayPeriodID, SUM(ROUND(CAST(DATEDIFF(minute,hrs.StartTime,hrs.EndTime) AS FLOAT)/60, 2)) AS TimeWorked
FROM	FWK_User	student	INNER JOIN	STF_TimecardEntry	logged	ON	student.ID				=		logged.EmployeeUserID
                            INNER JOIN	STF_TimecardHours	hrs		ON	logged.TimecardEntryID	=		hrs.TimecardEntryID
                                                                    AND	hrs.StartTime			IS NOT	NULL
                                                                    AND hrs.EndTime				IS NOT	NULL
GROUP BY	logged.PayPeriodID, student.HostID*1, student.FirstName, student.LastName
ORDER BY	student.LastName, student.FirstName

select * from STF_PayPeriodAdmin

SELECT new_id as id,fname,lname,q27,q28
FROM CUS_HousingAssignmentSurvey
WHERE (considerComments IS null OR considerComments LIKE 'Y')
AND (q27 NOT LIKE '' OR q28 NOT LIKE '')
AND CAST(q27 AS VARCHAR(MAX)) NOT IN ('')
AND year in (2013,0)
ORDER BY lname, fname DESC

select considerComments from CUS_HousingAssignmentSurvey where considerComments is not null

SELECT  student.Firstname, student.Lastname, student.HostID*1 AS ID, logged.PayPeriodID, CAST(hrs.StartTime AS DATE) AS theDay, SUM(ROUND(CAST(DATEDIFF(minute,hrs.StartTime,hrs.EndTime) AS FLOAT)/60, 2)) AS TimeWorked
FROM	FWK_User	student	INNER JOIN	STF_TimecardEntry	logged	ON	student.ID				=		logged.EmployeeUserID
                            INNER JOIN	STF_TimecardHours	hrs		ON	logged.TimecardEntryID	=		hrs.TimecardEntryID
                                                                    AND	hrs.StartTime			IS NOT	NULL
                                                                    AND hrs.EndTime				IS NOT	NULL
GROUP BY	CAST(hrs.StartTime AS DATE), logged.PayPeriodID, student.HostID*1, student.FirstName, student.LastName
ORDER BY	student.LastName, student.FirstName, theDay


            SELECT
                CASE    WHEN    A.AssignmentType    =   3   THEN    'true'
                                                            ELSE    'false'
                END             AS  'IsOnline'
            FROM
                CWK_Assignment  A
            WHERE
                CAST(A.AssignmentID AS NVARCHAR(40))  =   ''

select max(len(AssignmentID)) from CWK_Assignment