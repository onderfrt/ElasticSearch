﻿using Elasticsearch.Net;
using ElasticSearch.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public sealed class ValuesController : ControllerBase
    {
        AppDbContext context = new AppDbContext();
        [HttpGet("[action]")]

        public async Task<IActionResult> CreateData( CancellationToken cancellationToken)
        {
            IList<Person> persons = new  List<Person>();
            IList<Travel> travels = new List<Travel>();
            var random = new Random();
            for (var i = 0; i<10000; i++)
            {
                var title = new string(Enumerable.Repeat("abcdefgğhıijklmnoöprsştuüvyzx", 5)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var words = new List<string>();
                for (int j = 0; j < 300; j++)
                {
                    words.Add(new string(Enumerable.Repeat("abcdefgğhıijklmnoöprsştuüvyzx", 5)
                    .Select(s => s[random.Next(s.Length)]).ToArray()));
                }

                var description = string.Join(" ", words);
                var traver = new Travel()
                {
                    Title = title,
                    Description = description,
                };
                
                travels.Add(traver);
                
            }
            //await context.Set<Person>().AddRangeAsync(persons); 
            await context.Set<Travel>().AddRangeAsync(travels,cancellationToken); 
            await context.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        [HttpGet("[action]/{description}")]
        public async Task<IActionResult> GetDataListWithEF(string description)
        {
            IList<Travel> travels = await context.Set<Travel>()
                .Where(x => x.Description.Contains(description))
                .AsNoTracking()
                .ToListAsync();

            return Ok(travels.Take(10));
        }

        [HttpGet("[action]")]

        public async Task<IActionResult> SyncToResult()
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));

            var client = new ElasticLowLevelClient(settings);
            List<Travel> travels = await context.Travels.ToListAsync();

            var tasks = new List<Task>();
            foreach (var travel in travels)
            {
                tasks.Add(client.IndexAsync<StringResponse>("travels", travel.Id.ToString(),
                    PostData.Serializable(
                        new
                        {
                            travel.Id,
                            travel.Title,
                            travel.Description
                        }

                        )));
            }

            await Task.WhenAll(tasks);

            return Ok();
        }

        [HttpGet("[action]/{description}")]
        public async Task<IActionResult> GetDataListWithElasticSearch(string value)
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));
            var client = new ElasticLowLevelClient(settings);

            var response = await client.SearchAsync<StringResponse>("travels",
                PostData.Serializable(new
                {
                    query = new
                    {
                        wildcard = new
                        {
                           Description = new { value = $"*{value}*"}
                        }
                    }
                }));

            var results = JObject.Parse(response.Body);
            var hits = results["hits"]["hits"].ToObject<List<JObject>>();

            List<Travel> travels = new();
            
            foreach (var hit in hits) 
            {
                travels.Add(hit["_source"].ToObject<Travel>());
            }

            return Ok(travels.Take(10));
        }

    }
}
