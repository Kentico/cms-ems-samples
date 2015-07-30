Using WebAPI2 in Kentico
========================

[Related blog post](http://devnet.kentico.com/articles/using-asp-net-web-api-in-kentico-8-1)

[Download samples as a ZIP file](https://github.com/Kentico/Samples/archive/master.zip)

[Other options](https://github.com/Kentico/Samples)

Kentico 8.1 Web API Sample project (using Web API 2.0)

1. Extract the project folder to the same level of Kentico project where you have your CMS and Lib folders. Example:

    CMS
    Lib
    CustomWebAPI

2. Add such project to your Kentico solution.

3. Add reference of this 'CustomWebAPI' project to your Kentico solution into project references.

4. Install WebAPI 2.X into CustomWebAPI project using NuGet.

5. Create `Newtonsoft.Json.6.0.0.0` folder in the `CMS/CMSDependencies` and put there `Newtonsoft.Json.dll` in version `6.0.0.0`. This point assumes the version of Web API you use requires Newtonsoft.Json in version 6.0.0.0. You will need to update folder name in `CMSDependencies` and dll to always contain the same version your WebApi requires. This basically means that you will need to synchronize Newtonsoft.Json from `/packages` to `CMS/CMSDependencies` every time you update Web API. `CMSDepencies` is our special folder which allows us to use libraries even if they are not in bin. It is not intended for use by our customers, but in this special case it is the only way how to use two libraries at the same time which requires two different versions of the same assembly. 

6. Delete all dlls from `Lib/MVC` folder.

7. Remove CMSApp_MVC project from the solution or update it to MVC 5 in case you use it for MVC - please see documentation [how to update MVC](https://docs.kentico.com/display/K81/Upgrading+the+MVC+version)

8. Rebuild the solution.
