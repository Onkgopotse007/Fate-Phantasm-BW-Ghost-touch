# dotnet-7-rpg
This is a sample project I did to showcase my knowledge of .NET 7 feel free to reuse the code or even better give me pointers on improving my code.
My aim is to make an RPG based on the fate anime series, fate/zero and fate/stay night, I have settled on having 14 characters and have been able to balance their stats, been able to prevent players from choosing more than 1 character from the same class and have included one of the unique heroic spirits "Avenger" Angra Mainyu, I do think I might've maybe made it overkill by allowing a player to have 7 characters, all that's left for me is to balance their noble phantasms, then I will get into the game part of this project.

Hopefully this doesn't end in a cease and desist or copyright lawsuit 😂

There's two branches, if you want to make a simple crud app use development-crud-only while the complete project will be on development.

I'm on a break currently as I have to attend to work duties, I'll continue on the first of September as I aim to take some time off of programming (I only actually managed to take a break from programming for Christmas and new year only as the project I am employed at went to production in December).
Happy new year, finally managed to get more free time as I was juggling work and school previously but now I have managed to successfully complete my BSc Information Systems and Data Management at the Botswana International University of Science and Technology and graduation is in September, guess I'm technically degreeless till then🤣 The break took longer than I expected but in that time I was able to learn a lot more.

My first order of business now is to overhaul the role based access logic so I utilize a middleware based solution instead, after I have achieved that I will continue on to the functionality of the API. Otherwise happy coding and have a productive 2024. -BW Ghost-

Features
- Global exception handling middleware
- Validation of inputs using fluent validations
- Logging with serilogger
- Data transfer objects with Automapper
- Object Relational Mapping with EF core
- Bearer token authentication and authorization
- User roles to prevent access to some routes.

To build:
- dotnet build
- if it fails run dotnet restore first
To run
- dotnet watch run

I'm reachable on +267 75295351
