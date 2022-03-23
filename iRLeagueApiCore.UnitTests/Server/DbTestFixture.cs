using DbIntegrationTests;
using iRLeagueApiCore.Communication.Enums;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.UnitTests.Server
{
    public class DbTestFixture : IDisposable
    {
        private static IConfiguration Configuration { get; }

        private static readonly int Seed = 12345;
        public static string ClientUserName => "TestClient";
        public static string ClientGuid => "6a6a6e09-f4b7-4ccb-a8ae-f2fc85d897dd";


        private static IEnumerable<LeagueEntity> _leagues;
        /// <summary>
        /// Static list of leagues on the pre-populated test database. Will not change during tests
        /// </summary>
        public IEnumerable<LeagueEntity> Leagues => _leagues;
        /// <summary>
        /// Static list of members on the pre-populated test database. Will not change during tests
        /// </summary>
        public IEnumerable<MemberEntity> Members { get; private set; }
        /// <summary>
        /// Static list of seasons on the pre-populated test database. Will not change during tests
        /// </summary>
        public IEnumerable<SeasonEntity> Seasons { get; private set; }
        /// <summary>
        /// Static list of schedules on the pre-populated test database. Will not change during tests
        /// </summary>
        public IEnumerable<ScheduleEntity> Schedules { get; private set; }
        /// <summary>
        /// Static list of sessions on the pre-populated test database. Will not change during tests
        /// </summary>
        public IEnumerable<SessionEntity> Sessions { get; private set; }

        static DbTestFixture()
        {
            Configuration = ((IConfigurationBuilder)(new ConfigurationBuilder()))
                .AddUserSecrets<DbTestFixture>()
                .Build(); ;

            var random = new Random(Seed);

            // set up test database
            using (var dbContext = CreateStaticDbContext())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                Populate(dbContext, random);
                dbContext.SaveChanges();
                _leagues = dbContext.Leagues.ToList();
            }
        }

        public DbTestFixture()
        {
        }

        public static LeagueDbContext CreateStaticDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
            var connectionString = Configuration["ConnectionStrings:ModelDb"];

            // use in memory database when no connection string present
            optionsBuilder.UseMySQL(connectionString);

            var dbContext = new LeagueDbContext(optionsBuilder.Options);
            return dbContext;
        }

        public LeagueDbContext CreateDbContext()
        {
            return CreateStaticDbContext();
        }

        public ClaimsPrincipal User => new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "unitTestUser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, UserRoles.User),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));

        /// <summary>
        /// Add the default HttpContext to the provided controller
        /// </summary>
        /// <typeparam name="T">type of ApiController</typeparam>
        /// <param name="controller">Controller to add context</param>
        /// <returns>Controller with added context</returns>
        public T AddControllerContext<T>(T controller) where T : Controller
        {
            var user = User;
            var identity = (ClaimsIdentity)user.Identity;
            identity.AddClaim(new Claim(ClaimTypes.Role, $"{"TestLeague".ToLower()}_{UserRoles.User}"));
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        public T AddControllerContextWithoutLeagueRole<T>(T controller) where T : Controller
        {
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = User }
            };

            return controller;
        }

        public static void Populate(LeagueDbContext context, Random random)
        {
            // Populate Tracks
            for (int i = 0; i < 2; i++)
            {
                var group = new TrackGroupEntity()
                {
                    TrackName = $"Group{i}",
                    Location = "Testlocation"
                };
                for (int j = 0; j < 3; j++)
                {
                    var config = new TrackConfigEntity()
                    {
                        ConfigName = $"Config{i}",
                        ConfigType = ConfigType.RoadCourse,
                        Turns = j * 3,
                        LengthKm = j * 1.0,
                        MapImageSrc = null,
                        HasNigtLigthing = false
                    };
                    group.TrackConfigs.Add(config);
                }
                context.TrackGroups.Add(group);
            }

            // create models
            var league1 = new LeagueEntity()
            {
                Name = "TestLeague",
                NameFull = "League for unit testing"
            };
            var season1 = new SeasonEntity()
            {
                SeasonName = "Season One",
                CreatedOn = DateTime.Now,
                CreatedByUserName = ClientUserName,
                CreatedByUserId = ClientGuid,
                LastModifiedOn = DateTime.Now,
                LastModifiedByUserName = ClientUserName,
                LastModifiedByUserId = ClientGuid,
                League = league1
            };
            var season2 = new SeasonEntity()
            {
                SeasonName = "Season Two",
                CreatedOn = DateTime.Now,
                CreatedByUserName = ClientUserName,
                CreatedByUserId = ClientGuid,
                LastModifiedOn = DateTime.Now,
                LastModifiedByUserName = ClientUserName,
                LastModifiedByUserId = ClientGuid,
                League = league1
            };
            var schedule1 = new ScheduleEntity()
            {
                Name = "S1 Schedule",
                CreatedOn = DateTime.Now,
                CreatedByUserName = ClientUserName,
                CreatedByUserId = ClientGuid,
                LastModifiedOn = DateTime.Now,
                LastModifiedByUserName = ClientUserName,
                LastModifiedByUserId = ClientGuid
            };
            var schedule2 = new ScheduleEntity()
            {
                Name = "S2 Schedule 1",
                CreatedOn = DateTime.Now,
                CreatedByUserName = ClientUserName,
                CreatedByUserId = ClientGuid,
                LastModifiedOn = DateTime.Now,
                LastModifiedByUserName = ClientUserName,
                LastModifiedByUserId = ClientGuid
            };
            var schedule3 = new ScheduleEntity()
            {
                Name = "S2 Schedule 2",
                CreatedOn = DateTime.Now,
                CreatedByUserName = ClientUserName,
                CreatedByUserId = ClientGuid,
                LastModifiedOn = DateTime.Now,
                LastModifiedByUserName = ClientUserName,
                LastModifiedByUserId = ClientGuid
            };
            // Create sessions on schedule1
            for (int i = 0; i < 5; i++)
            {
                var session = new SessionEntity()
                {
                    Name = $"S1 Session {i + 1}",
                    CreatedOn = DateTime.Now,
                    CreatedByUserName = ClientUserName,
                    CreatedByUserId = ClientGuid,
                    LastModifiedOn = DateTime.Now,
                    LastModifiedByUserName = ClientUserName,
                    LastModifiedByUserId = ClientGuid,
                    Track = context.TrackGroups
                        .SelectMany(x => x.TrackConfigs)
                        .Skip(i)
                        .FirstOrDefault(),
                    SessionTitle = $"S1 Session {i + 1}",
                    SessionType = (SessionTypeEnum)i + 1
                };
                schedule1.Sessions.Add(session);
            }
            for (int i = 0; i < 2; i++)
            {
                var session = new SessionEntity()
                {
                    Name = $"S2 Session {i + 1}",
                    CreatedOn = DateTime.Now,
                    CreatedByUserName = ClientUserName,
                    CreatedByUserId = ClientGuid,
                    LastModifiedOn = DateTime.Now,
                    LastModifiedByUserName = ClientUserName,
                    LastModifiedByUserId = ClientGuid,
                    Track = context.TrackGroups
                        .SelectMany(x => x.TrackConfigs)
                        .Skip(i)
                        .FirstOrDefault(),
                    SessionTitle = $"S2 Session {i + 1}",
                    SessionType = (SessionTypeEnum)i + 1
                };
                schedule2.Sessions.Add(session);
            }
            context.Leagues.Add(league1);
            league1.Seasons.Add(season1);
            league1.Seasons.Add(season2);
            season1.Schedules.Add(schedule1);
            season2.Schedules.Add(schedule2);
            season2.Schedules.Add(schedule3);

            // create league2
            var league2 = new LeagueEntity()
            {
                Name = "TestLeague2",
                NameFull = "Second League for unit testing"
            };
            var l2season1 = new SeasonEntity()
            {
                SeasonName = "L2 Season One",
                CreatedOn = DateTime.Now,
                CreatedByUserName = ClientUserName,
                CreatedByUserId = ClientGuid,
                LastModifiedOn = DateTime.Now,
                LastModifiedByUserName = ClientUserName,
                LastModifiedByUserId = ClientGuid,
                League = league1
            };
            var l2schedule1 = new ScheduleEntity()
            {
                Name = "L2S1 Schedule 1",
                CreatedOn = DateTime.Now,
                CreatedByUserName = ClientUserName,
                CreatedByUserId = ClientGuid,
                LastModifiedOn = DateTime.Now,
                LastModifiedByUserName = ClientUserName,
                LastModifiedByUserId = ClientGuid
            };

            context.Leagues.Add(league2);
            league2.Seasons.Add(l2season1);
            l2season1.Schedules.Add(l2schedule1);

            GenerateMembers(context, random);

            // assign members to league
            foreach (var member in context.Members)
            {
                var leagueMember = new LeagueMemberEntity()
                {
                    Member = member,
                    League = league1
                };
                context.Set<LeagueMemberEntity>().Add(leagueMember);
            }

            var members = context.Members
                .Local
                .ToList();

            // create results
            var scoring = new ScoringEntity()
            {
                Name = "Testscoring",
                ShowResults = true
            };
            season1.Scorings.Add(scoring);

            foreach (var session in league1.Seasons.SelectMany(x => x.Schedules).SelectMany(x => x.Sessions))
            {
                var scoredResult = new ScoredResultEntity();
                scoring.ScoredResults.Add(scoredResult);
                var result = new ResultEntity();
                result.ScoredResults.Add(scoredResult);
                for (int i = 0; i < 10; i++)
                {
                    var resultRow = new ResultRowEntity()
                    {
                        StartPosition = i + 1,
                        FinishPosition = i + 1,
                        Member = members.ElementAt(i),
                        QualifyingTime = GetTimeSpan(random).Ticks,
                        FastestLapTime = GetTimeSpan(random).Ticks,
                        AvgLapTime = GetTimeSpan(random).Ticks,
                        Interval = GetTimeSpan(random).Ticks
                    };
                    var scoredResultRow = new ScoredResultRowEntity()
                    {
                        ResultRow = resultRow,
                        FinalPosition = i + 1,
                        RacePoints = 10 - i,
                        TotalPoints = 10 - i
                    };
                    result.ResultRows.Add(resultRow);
                    scoredResult.ScoredResultRows.Add(scoredResultRow);
                }
                scoring.Sessions.Add(session);
                session.Result = result;
            }
        }

        private static TimeSpan GetTimeSpan(Random random)
        {
            return new TimeSpan(0, 1, 2, 34, 567);
        }
        private static void GenerateMembers(LeagueDbContext context, Random random)
        {
            var minMemberCount = 50;
            var maxMemberCount = 100;

            var memberCount = random.Next(maxMemberCount - minMemberCount + 1) + minMemberCount;

            for (int i = 0; i < memberCount; i++)
            {
                var member = new MemberEntity()
                {
                    Firstname = GetRandomName(random),
                    Lastname = GetRandomName(random),
                    IRacingId = GetRandomIracingId(random)
                };
                context.Members.Add(member);
            }
        }

        private static string GetRandomName(Random random)
        {
            var minLen = 3;
            var len = random.Next(10) + minLen;
            char[] name = new char[len];
            char[] characters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ßÄÜÖäüö".ToCharArray();

            for (int i = 0; i < len; i++)
            {
                var offset = random.Next(characters.Length);
                name[i] = characters[offset];
            }
            return new string(name);
        }

        private static string GetRandomIracingId(Random random)
        {
            var len = 6;
            char[] id = new char[len];
            for (int i = 0; i < len; i++)
            {
                id[i] = (char)('0' + random.Next(10));
            }
            return new string(id);
        }

        public void Dispose()
        {
        }
    }
}
