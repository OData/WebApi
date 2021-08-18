# How to contribute?
There are many ways for you to contribute to OData Web API.  The easiest way is to participate in discussion of features and issues.  You can also contribute by sending pull requests of features or bug fixes to us.  Contribution to the documentations at our [GitHub Pages](http://odata.github.io/WebApi/) is also highly welcomed. 
##Discussion
You can participate into discussions and ask questions about OData Web API at our [GitHub issues](https://github.com/OData/WebApi/issues). 
###Bug reports
When reporting a bug at the issue tracker, please use the following template:
```
### Description
*Does the bug result in any actual functional issue, if so, what?*  

### Minimal repro steps
*What is the smallest, simplest set of steps to reproduce the issue. If needed, provide a project that demonstrates the issue.*  

### Expected result
*What would you expect to happen if there wasn't a bug*  

### Actual result
*What is actually happening*  

### Further technical details
*Optional, details of the root cause if known*  
```

## Pull requests
Pull request of features and bug fixes are both welcomed. Before you send a pull request to us, there are a few steps you need to make sure you've followed. 
### Complete a Contribution License Agreement (CLA)
You will need to complete a Contributor License Agreement (CLA). Briefly, this agreement testifies that you are granting us permission to use the submitted change according to the terms of the project's license, and that the work being submitted is under appropriate copyright.

Please submit a Contributor License Agreement (CLA) before submitting a pull request. Please fill and submit the [Contributor License Agreement](https://cla.dotnetfoundation.org/). Be sure to include your GitHub user name along with the agreement. Only after we have received the signed CLA, we'll review the pull request that you send. This needs to only be done once for any .NET Foundation OSS project.

### Create a new issue on the issue tracker and link the pull request to it
You should have an issue created on the [issue tracker](https://github.com/OData/WebApi/issues) before you work on the pull request. After the OData Web API team has reviewed this issue and change its label to "accepting pull request", you can issue a pull request to us in which the link to the related issue is included.
### Requirement of pull requests
Your pull request should:

 - Include a description of what your change intends to do
 - Have clear commit messages
 - Include a link to the issue created at the issue tracker or its issue number
 - Include adequate function tests, corresponding E2E tests
 - Pass all tests without error

### Run test
Function test
```
build quick
```

Or you can just run all test
```
build
```
