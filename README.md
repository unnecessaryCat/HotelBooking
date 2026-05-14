This application has been built with .net 10 and a docker file for running SQL Server.

Notes:
Ran out of time to host this in Azure but if I was going to host this I would
1. Create an Azure SQL DB to contain my DB
2. Create an Azure App Service (could do a container app but overkill for a simple case)
3. Create a github action and a deploy.yml file to deploy the changes

AI usage:
I initially used Claude in plan mode to plan out the work, considered error cases and writing tests in a TDD approach. After reviewing I allowed claude to implement the initial code. From there I re-reviewed in detail and made adjustments.


Known Limitation:
1. Currenntly no delete endpoints exist nor can you modify the bookings, rooms or hotels which is a bit annoying

Things not considered but would be interesting longer term:
1. How would this expand? Could our hotels suddenly have 100's or 1000's of rooms, could we have a large number of hotels - check that queries are performant
2. How would we manage scale? Could this be seen as something to service many users in real time?
3. What about the customers? Should we have a login system and then a booking could foreign key off that and a user could search for all their bookings just by looking at their foreign keys., 
