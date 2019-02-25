using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hangfire_api.Models;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Hangfire;
using MySql.Data.MySqlClient;


namespace hangfire_api.Controllers {
    [Produces("application/json")]
    [Route("api/hangfire")]
    [ApiController]
    public class gitRepo_Controller : ControllerBase 
    {
        private readonly gitRepo_Context _Context;
        private IHttpClientFactory _clientFactory;

        public gitRepo_Controller(IHttpClientFactory clientFactory, gitRepo_Context context) {
            _clientFactory = clientFactory;
            _Context = context;

            RecurringJob.AddOrUpdate(() => this.pullRepos(), Cron.MinuteInterval(10));
        }

        [HttpGet]
        public async Task<IActionResult> pullRepos() {
            
            // Use the github client created on the startup class !
            var client = _clientFactory.CreateClient("github");
            //Declare a null httpresponse for later use 
            HttpResponseMessage response = null;
            try {
                // retrive the repos from github 
                response = await client.GetAsync("/users/WickedSs/repos");
            } catch (HttpRequestException e) {
                return BadRequest(Response);
            }
            if (response.IsSuccessStatusCode) {
                List<gitRepo> Remoteprojects = (await response.Content.ReadAsAsync<List<gitRepo>>());
                List<gitRepo> localProjects;
                try {
                    localProjects = await getLocal();
                } catch (MySqlException e) {
                    return StatusCode(e.ErrorCode, e.Message);
                }

                List<gitRepo> delete = deleteRecords(Remoteprojects, localProjects);
                List<gitRepo> add = addRecords(Remoteprojects, localProjects);
                _Context.RemoveRange(delete);
                _Context.AddRange(add);
                try {
                    // Save the changes to the database, then print then to the route /api/hangfire  
                    int whatChanged = await _Context.SaveChangesAsync();
                    string updated = printUpdated(add, delete);
                    printUpdated(add, delete);
                    return Ok(new {changes = whatChanged, updates = updated});
                } catch (DbUpdateException e) {
                    return StatusCode(500, new {message = e.Message, HelpLink = e.HelpLink});
                }
            } else {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
        // declared it here so i can access it in both funcs 
        gitRepoComparator cmpfunc = new gitRepoComparator();


        // Prevent replicate records in the database !
        public List<gitRepo> deleteRecords(List<gitRepo> remote, List<gitRepo> local) {

            List<gitRepo> removeL = new List<gitRepo>();

            for (var i = 0; i < local.Count; i++) {
                gitRepo currlocal = local[i];
                int currIndex = remote.BinarySearch(currlocal, this.cmpfunc);
                if (currIndex < 0) {
                    removeL.Add(currlocal);
                } else {
                    gitRepo curRemote = remote[currIndex];
                    if (!currlocal.Equals(curRemote)) {
                        removeL.Add(currlocal);
                    } 
                }
            }
            return removeL;
        }


        // retrive the repository's Id from teh response for later comparison 
        public string printgitReposList(List<gitRepo> repos) {
            string str = "";
            for (int i = 0; i < repos.Count; i++) {
                    str += repos[i].id;
            }
            return str + "\n";
        }

        // print the updated records Id's in the route /api/hangfire
        public string printUpdated(List<gitRepo> add, List<gitRepo> delete) {
            string str = " ======== Updates ========== <br> ---------- deleted ----------";
            str += printgitReposList(delete) + "<br>";
            str += "-------- Added --------- <br>" ;
            str += printgitReposList(add);
            return str;

        }

        // add data we get from github of each repository 
        // check the Model for the data from the response
        public List<gitRepo> addRecords(List<gitRepo> remote, List<gitRepo> local) {
            List<gitRepo> addL = new List<gitRepo>();

            for (var i = 0; i < remote.Count; i++) {
                gitRepo currRemote = remote[i];
                int currIndex = local.BinarySearch(currRemote, this.cmpfunc);
                if (currIndex < 0) {
                    addL.Add(currRemote);
                } else {
                    gitRepo currlocal = local[currIndex];
                    if (!currRemote.Equals(currlocal)) {
                        addL.Add(currRemote);
                    } 
                }
            }
            return addL;
        }

        
        // retrive the records existed in the database 
        private async Task<List<gitRepo>> getLocal() {
            return await _Context.gitRepos.ToListAsync();
        }
    }


    // comparison dunction the compare Repository Id's
    public class gitRepoComparator : IComparer<gitRepo>
    {
        public int Compare(gitRepo A, gitRepo B) {
            return A.id.CompareTo(B.id);
        }
        
    }
}