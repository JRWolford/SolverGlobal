# Introduction
Hello all! Welcome to my rough attempt to complete the requisite coding challenge. In keeping with the theme of being brief but thourough you will find that there are only two projects in the solution, _Api_ and _Api.Unit.Tests_. It should be pretty straight forward but the first one is the actual code for the API and the second one are the few unit tests I wrote for it.

# Setup

## Setup Using Docker
Assuming you have Docker setup on your local machine, you can make use of the **/build/deploy-dbs.cmd** file which will setup the **docker-compose.yml** and seed any databases as necessary. The connection strings should already be established inside of your **appsettings.Development.json**.

If you run into issues with your port mappings, feel free to modify them, but be aware that you will have to verify the associated connection string is setup.

## Setup Using Available Network Connections
To set this up on your local machine using databases available on your network, simply add a connection string record to the **appsettings.Development.json** file to a database that you have access to. Seeing as this has been whacked together pretty quickly I would suggest using a local throwaway database.

```json
{
    "ConnectionStrings": {
        "Your-Database": "<Your-Database-Connection-String>"
    }
}
```

The only requisite is that the database connection string is for some sort of SQL Server database, as I have not written this to work for something like Oracle or Postgres.

# Running and Calling
I just used Swagger to create a simple UI to test out the API, and it is possible to simply build and run the solution and then open up swagger in the browser. Of course you could also use a tool like PostMan to call it if you wanted.

Assuming that you have a database connection string setup as shown in the previous section, you could grab data from the database by calling:

```
GET
/api/Your-Database/Your-Table/data
```

Assuming, `Your-Table`, is a valid table name for your database then you should quickly see some data returned.

Bear in mind that I did not optimize this at all really so large data sets are going to take a while.

# Design Philosophy
So the challenge was actually pretty fun for me. Normally I have a concrete idea of what database I need, what tables it has, what the design of those tables are, and more specifically what data I need.

Herein I talk about the various thoughts and ideas I had about the design, broken out by main component.

## Repository Design
As I mentioned, I typically have a more concrete idea of the data sources I am using.

To that end I usually like to make use of EF Core as it is very simple to setup and makes unit testing your repositories a breeze. I didn't go that route here as EF Core really likes you to have a well defined `DbSet<T>` so instead I went with **Sytstem.Data.SqlClient** as it is more flexible.

The typical architectural patterns I use when setting up a database repository are the repository and unit of work patterns. I did not go that crazy here as at the core of it we are just doing a simple `SELECT * FROM TABLE`, but I did try to demonstrate how I would use the repository pattern and dependency injection to create a framework that is not tightly coupled to a database. I think I have a pretty decent framework in place that could easily be expanded on to incorporate data sources other than SQL Server, such as Oracle or something.

The main concern that I had with my design was how is the system going to respond to a SQL Injection attack? To reduce the risk I implemented a method that checks to see if the database exists or not. I did this using SQL parameters, so that I remove the risk of injection attacks. You can find the actual implementation of this in the **SqlServerDataRepository**, line 39.

## Controller Design
I honestly didn't do anything too crazy with the controller design. My main focus was in creating a simple endpoint that met the criteria of the challenge.

The one thing that I did want to design for was writing the API response in such a way that an attacker couldn't just comb through a list of database and table names to try and map out the various data sources this API could expose. To that end I made the controller only return 200 OK results as the attacker wouldn't be able to guess whether they database and table name combination resulted in a successful hit or not, they would have to look at the response body which would at least slow the attack down.

### Controller Handlers
So I like to write my controllers to make use of handlers when writing an API controller so that the endpoint code is simple orchestration and has little or no business logic in it.

I created a few handlers for this, but they are really all just part of a chain of command pattern so that, in the future, we could add a new link to the chain and immediately expand the functionality of the system.

The main three handlers that I have are:
 - CachedDataHandler
 - SqlServerDataHandler
 - NoValidDataHandler

 The **CachedDataHandler** gets called first and it just looks to see if the table data for the specified database is cached or not and returns it if it is. This was to demonstrate that we could easily use a cache system (in this case an InMemory cache but could be refactored to be Redis or something else) to speed up repeat request responses.

 The **SqlServerDataHandler** is the main meat and potatoes of the handler system as it is what is actually used to get data from a SQL server database. You can look at that handler and see how we could easily add a handler that went and got data from Oracle, or maybe from another API, or possibly from Excel or a CSV file. The point is the chain of command pattern was my solution for allowing the API to easily support mutliple data sources

 The **NoValidDataHandler** is a special handler that is meant to sit at the end of the chain and basically say, "Hey we don't have a handler that can process this request". Its kind of redundant but I made it so that I could have a spot where I could possibly add some extra logging or alerting so that if we ever had a request that hit that handler we would know about it.

# Testing Philosophy
So upfront, I did not write tests for everything. One, I was lazy on a Saturday. And two, I really figured that you guys wouldn't want to comb over a bunch of unit tests and that the few that I added were more than enough to give you an idea of how I write tests.

I typically like to shoot for better code coverage than what I have in here but, again, demo.

## Third-Party Testing Frameworks
I made use of XUnit, Moq, and AutoFixture/AutoMoq. I really like using AutoMoq whenever I can as it makes setting up and configuring dependencies for unit tests a breeze.