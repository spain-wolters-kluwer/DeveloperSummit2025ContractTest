# 1. Use Case Bad Implementation

This is an example of what happens when the services are developed without taking care to maintain the interfaces for all consumers of the **UserPermissions** service.
In this case the **Blog** service works fine, but the users can't get the **Weather Forecast** because this API fails. 

The code is in the **course/01BadImplementation** branch.

In this case:

User John Doe can read and create articles in Blog Service
User Jane Doe can only read articles in Blog Service

But both users can access to WeatherForecast Service due to an bug.