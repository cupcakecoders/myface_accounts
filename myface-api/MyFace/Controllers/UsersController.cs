using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using MyFace.Models.Request;
using MyFace.Models.Response;
using MyFace.Repositories;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Primitives;
using MyFace.Data;
using MyFace.Models.Database;

namespace MyFace.Controllers
{
    [ApiController]
    [Route("/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepo _users;

        public UsersController(IUsersRepo users)
        {
            _users = users;
        }

        [HttpGet("")]
        public ActionResult<UserListResponse> Search([FromQuery] SearchRequest searchRequest)
        {
            var users = _users.Search(searchRequest);
            var userCount = _users.Count(searchRequest);
            return UserListResponse.Create(searchRequest, users, userCount);
        }

        [HttpGet("{id}")]
        public ActionResult<UserResponse> GetById([FromRoute] int id)
        {
            var authHeader = Request.Headers["Authorization"];
            
            if (String.IsNullOrEmpty(authHeader).Equals(false) && authHeader.ToString().Contains("Basic"))
            {
                StringValues encodedUsernamePassword = authHeader.ToString().Remove(0, 5);
                var usernamePassword = Convert.FromBase64String(encodedUsernamePassword);
                string decoded = Encoding.UTF8.GetString(usernamePassword);
                var splitDecodedUnPwd = decoded.Split(":");
                var userQueryUsername = _users.Where(splitDecodedUnPwd[0]);
                // if no user with username is found then return
                var getHashedPassword = userQueryUsername.HashedPassword;
                var getUserSalt = userQueryUsername.Salt;
                var decodedPassword = splitDecodedUnPwd[1];
                var decodedPasswordHashed = HashSalt.HashPassword(decodedPassword, getUserSalt);
                
                if (getHashedPassword.Equals(decodedPasswordHashed) && id.Equals(userQueryUsername.Id))
                {
                    var user = _users.GetById(userQueryUsername.Id);
                    return new UserResponse(user);
                }
                else
                {
                    return Unauthorized("bad login");
                }

            } else {
                // should be a 400 bad request
                throw new Exception("The authorization header is either empty or isn't Basic.");
            }
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateUserRequest newUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = _users.Create(newUser);

            var url = Url.Action("GetById", new { id = user.Id });
            var responseViewModel = new UserResponse(user);
            return Created(url, responseViewModel);
        }

        [HttpPatch("{id}/update")]
        public ActionResult<UserResponse> Update([FromRoute] int id, [FromBody] UpdateUserRequest update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _users.Update(id, update);
            return new UserResponse(user);
        }
        
        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            _users.Delete(id);
            return Ok();
        }
    }
}