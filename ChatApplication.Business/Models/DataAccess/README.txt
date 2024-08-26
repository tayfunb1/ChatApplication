In a real-life scenario this folder(DataAccess) and its contents(entities, DbContext, migrations, repositories etc.) 
would be in a different project/layer(Domain/DataAccess), but since I did not think it was necessary to use an 
in-memory database for this project, I omitted this part and put Entities inside this folder because 
it didn't make sense to create a class library just to hold a couple of classes :)