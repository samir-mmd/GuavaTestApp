using System;
using Xunit;
using GuavaStart;
using GuavaStart.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace GuavaStartUnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void GetCurrenWSUserId()
        {
            GuavaNotificationController notificationController = new GuavaNotificationController();

            // Act
            var result = notificationController.GetWsUserID();

            // Assert
            Assert.IsType<Int32>(result);
        }


        [Fact]
        public void UserRegistrationFormTest()
        {
            GuavaUserController guavaUserController = new GuavaUserController();

            // Act
            var result = guavaUserController.Register("testusername","testuserpassword");

            // Assert
            Assert.IsAssignableFrom<StatusCodeResult>(result);
        }

        [Fact]
        public void RegisterUser()
        {
            //Arrange
            GuavaUserEngine.Model.GuavaUser guavaUser = new GuavaUserEngine.Model.GuavaUser();
            GuavaUserEngine.Model.UserAuthMessage userAuthMessage = new GuavaUserEngine.Model.UserAuthMessage();
            guavaUser.userName = "UniqueTestUserName" + Guid.NewGuid().ToString();
            guavaUser.Password = "TestPassword";
            userAuthMessage.guavaUser = guavaUser;

            // Act
            GuavaUserEngine.Program.RegisterUser(userAuthMessage);
            var successResult = userAuthMessage.result;
            GuavaUserEngine.Program.RegisterUser(userAuthMessage);
            var errorResult = userAuthMessage.result;

            // Assert
            Assert.Equal("Success", successResult);
            Assert.Equal("User Already Exists", errorResult);
        }


        [Fact]
        public void AutorizeUser()
        {
            //Arrange
            GuavaUserEngine.Model.GuavaUser guavaUser = new GuavaUserEngine.Model.GuavaUser();
            GuavaUserEngine.Model.UserAuthMessage userAuthMessage = new GuavaUserEngine.Model.UserAuthMessage();
            guavaUser.userName = "UniqueTestUserName" + Guid.NewGuid().ToString();
            guavaUser.Password = "TestPassword";
            userAuthMessage.guavaUser = guavaUser;

            // Act
            GuavaUserEngine.Program.AutorizeUser(userAuthMessage);
            var userNotFoundResult = userAuthMessage.result;

            GuavaUserEngine.Program.RegisterUser(userAuthMessage);
            GuavaUserEngine.Program.AutorizeUser(userAuthMessage);
            var successResult = userAuthMessage.result;

            userAuthMessage.guavaUser.Password = "WrongPassword";
            GuavaUserEngine.Program.AutorizeUser(userAuthMessage);
            var wrongPasswordResult = userAuthMessage.result;

            // Assert
            Assert.Equal("User not found", userNotFoundResult);          
            Assert.Equal("Success", successResult);
            Assert.Equal("Wrong Password", wrongPasswordResult);
        }





    }
}
