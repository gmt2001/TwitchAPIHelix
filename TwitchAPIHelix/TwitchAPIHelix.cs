/*
 *  TwitchAPIHelix - A library to access the Twitch Helix API for .NET 4.7/Mono
 *  Copyright (C) 2018 gmt2001 - https://keybase.io/gmt2001
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published
 *  by the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchAPIHelix
{
    /// <summary>
    /// Executes requests using the Twitch Helix API
    /// </summary>
    public class TwitchAPIHelix
    {
        /// <summary>
        /// Base URL for Helix requests
        /// </summary>
        private static readonly string base_url = "https://api.twitch.tv/helix/";

        /// <summary>
        /// Unix epoch
        /// </summary>
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Lock object for GetData
        /// </summary>
        private static readonly object getDataLock = new object();

        /// <summary>
        /// Request timeout
        /// </summary>
        private static readonly int timeout = 5 * 1000;

        /// <summary>
        /// User agent for requests
        /// </summary>
        private static readonly string user_agent = "TwitchAPIHelix/1.0";

        /// <summary>
        /// Either a valid Client ID or a valid OAuth token from Twitch
        /// </summary>
        private readonly string clientidOrOauth;

        /// <summary>
        /// If true, clientidOrOauth is an OAuth token
        /// </summary>
        private readonly bool isOauth;

        /// <summary>
        /// The rate-limit assigned by Twitch
        /// </summary>
        private int ratelimit_limit = 30;

        /// <summary>
        /// Remaining requests within the current rate-limit window
        /// </summary>
        private int ratelimit_remaining = 30;

        /// <summary>
        /// When the rate-limit window resets
        /// </summary>
        private DateTime ratelimit_reset = DateTime.UtcNow;

        /// <summary>
        /// Valid HTTP methods for requests
        /// </summary>
        private enum Request_type
        {
            GET, POST, PUT, DELETE
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientidOrOauth">Either a valid Client ID or a valid OAuth token from Twitch</param>
        /// <param name="isOauth">Set to true if the first parameter contains an OAuth token</param>
        public TwitchAPIHelix(string clientidOrOauth, bool isOauth)
        {
            this.clientidOrOauth = clientidOrOauth.Replace("oauth:", "");
            this.isOauth = isOauth;
        }

        /// <summary>
        /// Executes an API call and returns the result
        ///
        /// <para>If using the ClientID or OAuth provided to the constructor, enforces the rate-limit</para>
        /// </summary>
        /// <param name="type">The HTTP method to use</param>
        /// <param name="url">The URL to query</param>
        /// <param name="post">If set, post data to send</param>
        /// <param name="isJson">If true, the post data is JSON</param>
        /// <param name="oauth">If set, overrides the OAuth token to use for the request</param>
        /// <returns>The result from Twitch</returns>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if both <see cref="TwitchAPIHelix.clientidOrOauth"/> and
        /// <paramref name="oauth"/> are not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        private string GetData(Request_type type, string url, string post, bool isJson, string oauth)
        {
            if (oauth != null && oauth.Length > 0)
            {
                goto ExecuteRequest;
            }

            goto CheckLimit;
        WaitForLimit:
            if (this.ratelimit_reset.CompareTo(DateTime.UtcNow) > 0)
            {
                Thread.Sleep(this.ratelimit_reset - DateTime.UtcNow);
            }

            lock (TwitchAPIHelix.getDataLock)
            {
                if (this.ratelimit_remaining == 0)
                {
                    this.ratelimit_remaining = this.ratelimit_limit;
                    this.ratelimit_reset = DateTime.UtcNow.AddSeconds(60);
                }
            }

            Thread.Sleep(300);

        CheckLimit:
            lock (TwitchAPIHelix.getDataLock)
            {
                if (this.ratelimit_remaining > 0)
                {
                    this.ratelimit_remaining--;
                    goto ExecuteRequest;
                }
                else
                {
                    goto WaitForLimit;
                }
            }

        ExecuteRequest:
            string ret = "";

            if (this.clientidOrOauth.Length == 0 && (oauth == null || oauth.Length == 0))
            {
                throw new Exceptions.AuthorizationRequiredException();
            }

            try
            {
                url += url.Contains("?") ? "&" : "?";

                url += "tick=" + (int)Math.Floor(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30.0);

                Uri uri = new Uri(url);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);

                req.ContentType = isJson ? "application/json" : "application/x-www-form-urlencoded";

                if (oauth != null && oauth.Length > 0)
                {
                    req.Headers.Add("Authorization", "Bearer " + oauth.Replace("oauth:", ""));
                }
                else if (this.isOauth)
                {
                    req.Headers.Add("Authorization", "Bearer " + this.clientidOrOauth);
                }
                else
                {
                    req.Headers.Add("Client-ID", this.clientidOrOauth);
                }

                req.Method = type.ToString();
                req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                req.Timeout = timeout;
                req.UserAgent = user_agent;
                req.KeepAlive = true;

                if (isJson && post.Length == 0)
                {
                    post = "{}";
                }

                if (post.Length > 0)
                {
                    Stream requestStream = req.GetRequestStream();
                    requestStream.ReadTimeout = timeout;
                    requestStream.WriteTimeout = timeout;

                    using (StreamWriter reqStream = new StreamWriter(requestStream))
                    {
                        reqStream.Write(post);
                    }
                }

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    Stream responseStream = res.GetResponseStream();
                    responseStream.ReadTimeout = timeout;
                    responseStream.WriteTimeout = timeout;

                    if (oauth == null || oauth.Length == 0)
                    {
                        int.TryParse(res.Headers.Get("Ratelimit-Limit"), out this.ratelimit_limit);
                        int.TryParse(res.Headers.Get("Ratelimit-Remaining"), out this.ratelimit_remaining);
                        long.TryParse(res.Headers.Get("Ratelimit-Reset"), out long ts);

                        this.ratelimit_limit = Math.Max(this.ratelimit_limit, 30);
                        this.ratelimit_reset = epoch.AddSeconds(ts);
                    }

                    using (StreamReader resStream = new StreamReader(responseStream))
                    {
                        ret = resStream.ReadToEnd();
                    }

                    if ((int)res.StatusCode < 200 || (int)res.StatusCode > 299)
                    {
                        DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(TwitchError));

                        MemoryStream ms = null;
                        TwitchError te;

                        try
                        {
                            ms = new MemoryStream(Encoding.UTF8.GetBytes(ret))
                            {
                                Position = 0
                            };
                            te = (TwitchError)js.ReadObject(ms);
                        }
                        finally
                        {
                            if (ms != null)
                            {
                                ms.Dispose();
                            }
                        }

                        throw new Exceptions.TwitchErrorException(te.status + ": " + te.message);
                    }
                }
            }
            catch (System.Exception e)
            {
                if (e.GetType() == typeof(WebException))
                {
                    if (((WebException)e).Status == WebExceptionStatus.ProtocolError)
                    {
                        using (HttpWebResponse res = (HttpWebResponse)((WebException)e).Response)
                        {
                            Stream responseStream = res.GetResponseStream();
                            responseStream.ReadTimeout = timeout;
                            responseStream.WriteTimeout = timeout;

                            using (StreamReader resStream = new StreamReader(responseStream))
                            {
                                ret = resStream.ReadToEnd();
                            }

                            if ((int)res.StatusCode < 200 || (int)res.StatusCode > 299)
                            {
                                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(TwitchError));

                                MemoryStream ms = null;
                                TwitchError te;

                                try
                                {
                                    ms = new MemoryStream(Encoding.UTF8.GetBytes(ret))
                                    {
                                        Position = 0
                                    };
                                    te = (TwitchError)js.ReadObject(ms);
                                }
                                finally
                                {
                                    if (ms != null)
                                    {
                                        ms.Dispose();
                                    }
                                }

                                throw new Exceptions.TwitchErrorException(te.status + ": " + te.message);
                            }
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets game information by game ID or name
        ///
        /// <para>For a query to be valid, <paramref name="name"/> and/or <paramref name="id"/> must be specified</para>
        /// </summary>
        /// <param name="id">Game ID. At most 100 id values can be specified</param>
        /// <param name="name">Game name. The name must be an exact match. For instance,
        /// “Pokemon” will not return a list of Pokemon games; instead, query the specific
        /// Pokemon game(s) in which you are interested. At most 100 name values can be specified</param>
        /// <returns>An array of matching games</returns>
        /// <exception cref="System.ArgumentException">Thrown if both <paramref name="id"/> and <paramref name="name"/> are not provided or
        /// have more than 100 entries</exception>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Games.GamesList GetGames(string[] id, string[] name)
        {
            if ((id == null || id.Length == 0 || id[0].Length == 0) && (name == null || name.Length == 0 || name[0].Length == 0))
            {
                throw new ArgumentException("Must provide at least one name or id");
            }

            if ((id != null && id.Length > 100) || (name != null && name.Length > 100))
            {
                throw new ArgumentException("Maximum of 100 entries allowed for id or name parameter");
            }

            Games.GamesList obj;

            try
            {
                string param = id != null && id.Length > 0 ? "id=" + string.Join("&id=", id) : "";
                param += param.Length > 0 && name != null && name.Length > 0 ? "&" : "";
                param += name != null && name.Length > 0 ? "name=" + string.Join("&name=", name) : "";

                string data = this.GetData(Request_type.GET, TwitchAPIHelix.base_url + "games?" + param, "", false, null);

                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Games.GamesList));

                MemoryStream ms = null;

                try
                {
                    ms = new MemoryStream(Encoding.UTF8.GetBytes(data))
                    {
                        Position = 0
                    };
                    obj = (Games.GamesList)js.ReadObject(ms);
                }
                finally
                {
                    if (ms != null)
                    {
                        ms.Dispose();
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }

            return obj;
        }

        /// <summary>
        /// Gets game information by game ID or name
        ///
        /// <para>For a query to be valid, <paramref name="name"/> and/or <paramref name="id"/> must be specified</para>
        /// </summary>
        /// <param name="id">Game ID. At most 100 id values can be specified</param>
        /// <param name="name">Game name. The name must be an exact match. For instance,
        /// “Pokemon” will not return a list of Pokemon games; instead, query the specific
        /// Pokemon game(s) in which you are interested. At most 100 name values can be specified</param>
        /// <returns>An array of matching games</returns>
        /// <exception cref="System.ArgumentException">Thrown if both <paramref name="id"/> and <paramref name="name"/> are not provided or
        /// have more than 100 entries</exception>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Task<Games.GamesList> GetGamesAsync(string[] id, string[] name) => Task.Run<Games.GamesList>(() => this.GetGames(id, name));

        /// <summary>
        /// Gets games sorted by number of current viewers on Twitch, most popular first
        /// </summary>
        /// <param name="first">Maximum number of objects to return. Maximum: 100. Default: 20</param>
        /// <param name="after">(Optional) Cursor for forward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="before">(Optional) Cursor for backward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <returns>An array of matching games</returns>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Games.GamesList GetTopGames(int first,string after, string before)
        {
            Games.GamesList obj;

            first = Math.Min(100, Math.Max(1, first));

            try
            {
                string param = after != null && after.Length > 0 ? "after=" + after : before != null && before.Length > 0 ? "before=" + before : "";
                param += param.Length > 0 ? "&" : "";
                param += "first=" + first;

                string data = this.GetData(Request_type.GET, TwitchAPIHelix.base_url + "games/top?" + param, "", false, null);

                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Games.GamesList));

                MemoryStream ms = null;

                try
                {
                    ms = new MemoryStream(Encoding.UTF8.GetBytes(data))
                    {
                        Position = 0
                    };
                    obj = (Games.GamesList)js.ReadObject(ms);
                }
                finally
                {
                    if (ms != null)
                    {
                        ms.Dispose();
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }

            return obj;
        }

        /// <summary>
        /// Gets games sorted by number of current viewers on Twitch, most popular first
        /// </summary>
        /// <param name="first">Maximum number of objects to return. Maximum: 100. Default: 20</param>
        /// <param name="after">(Optional) Cursor for forward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="before">(Optional) Cursor for backward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <returns>An array of matching games</returns>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Task<Games.GamesList> GetTopGamesAsync(int first, string after, string before) => Task.Run<Games.GamesList>(() => this.GetTopGames(first, after, before));

        /// <summary>
        /// Gets a ranked list of Bits leaderboard information for an authorized broadcaster
        /// 
        /// <para>Required scope: bits:read</para>
        /// </summary>
        /// <param name="count">Number of results to be returned. Maximum: 100. Default: 10</param>
        /// <param name="period">(Optional) Time period over which data is aggregated (PST time zone).
        /// This parameter interacts with started_at. Valid values are given below. Default: "all".
        /// <para>"day" - 00:00:00 on the day specified in started_at, through 00:00:00 on the following day. /////
        /// "week" - 00:00:00 on Monday of the week specified in started_at, through 00:00:00 on the following Monday. /////
        /// "month" - 00:00:00 on the first day of the month specified in started_at, through 00:00:00 on the first day of the following month. /////
        /// "year" - 00:00:00 on the first day of the year specified in started_at, through 00:00:00 on the first day of the following year. /////
        /// "all" - The lifetime of the broadcaster's channel. If this is specified (or used by default), started_at is ignored</para></param>
        /// <param name="started_at">(Optional) Timestamp for the period over which the returned data is aggregated. Must be in RFC 3339 format. If this is not
        /// provided, data is aggregated over the current period; e.g., the current day/week/month/year. This value is ignored if period is "all".
        /// Any + operator should be URL encoded</param>
        /// <param name="user_id">(Optional) ID of the user whose results are returned; i.e., the person who paid for the Bits.
        /// If user_id is not provided, the endpoint returns the Bits leaderboard data across top users (subject to the value of count)</param>
        /// <param name="oauth">(Optional) If provided, overrides <see cref="TwitchAPIHelix.clientidOrOauth"/></param>
        /// <returns>An array of bits leaderboard entries</returns>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> or
        /// <paramref name="oauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Bits.BitsLeaderboard GetBitsLeaderboard(int count, string period, string started_at, string user_id, string oauth)
        {
            Bits.BitsLeaderboard obj;
            
            count = Math.Min(100, Math.Max(1, count));

            try
            {
                string param = period != null && period.Length > 0 ? "period=" + period : "";
                param += param.Length > 0 ? "&" : "";
                param += started_at != null && started_at.Length > 0 ? "started_at=" + started_at : "";
                param += param.Length > 0 ? "&" : "";
                param += user_id != null && user_id.Length > 0 ? "user_id=" + user_id : "";
                param += "count=" + count;

                string data = this.GetData(Request_type.GET, TwitchAPIHelix.base_url + "bits/leaderboard?" + param, "", false, oauth);

                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Bits.BitsLeaderboard));

                MemoryStream ms = null;

                try
                {
                    ms = new MemoryStream(Encoding.UTF8.GetBytes(data))
                    {
                        Position = 0
                    };
                    obj = (Bits.BitsLeaderboard)js.ReadObject(ms);
                }
                finally
                {
                    if (ms != null)
                    {
                        ms.Dispose();
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }

            return obj;
        }

        /// <summary>
        /// Gets a ranked list of Bits leaderboard information for an authorized broadcaster
        /// 
        /// <para>Required scope: bits:read</para>
        /// </summary>
        /// <param name="count">Number of results to be returned. Maximum: 100. Default: 10</param>
        /// <param name="period">(Optional) Time period over which data is aggregated (PST time zone).
        /// This parameter interacts with started_at. Valid values are given below. Default: "all".
        /// <para>"day" - 00:00:00 on the day specified in started_at, through 00:00:00 on the following day. /////
        /// "week" - 00:00:00 on Monday of the week specified in started_at, through 00:00:00 on the following Monday. /////
        /// "month" - 00:00:00 on the first day of the month specified in started_at, through 00:00:00 on the first day of the following month. /////
        /// "year" - 00:00:00 on the first day of the year specified in started_at, through 00:00:00 on the first day of the following year. /////
        /// "all" - The lifetime of the broadcaster's channel. If this is specified (or used by default), started_at is ignored</para></param>
        /// <param name="started_at">(Optional) Timestamp for the period over which the returned data is aggregated. Must be in RFC 3339 format. If this is not
        /// provided, data is aggregated over the current period; e.g., the current day/week/month/year. This value is ignored if period is "all".
        /// Any + operator should be URL encoded</param>
        /// <param name="user_id">(Optional) ID of the user whose results are returned; i.e., the person who paid for the Bits.
        /// If user_id is not provided, the endpoint returns the Bits leaderboard data across top users (subject to the value of count)</param>
        /// <param name="oauth">(Optional) If provided, overrides <see cref="TwitchAPIHelix.clientidOrOauth"/></param>
        /// <returns>An array of bits leaderboard entries</returns>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> or
        /// <paramref name="oauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Task<Bits.BitsLeaderboard> GetBitsLeaderboardAsync(int count, string period, string started_at, string user_id, string oauth) =>
            Task.Run<Bits.BitsLeaderboard>(() => this.GetBitsLeaderboard(count, period, started_at, user_id, oauth));

        /// <summary>
        /// Gets information about active streams. Streams are returned sorted by number of current viewers, in descending order.
        /// Across multiple pages of results, there may be duplicate or missing streams, as viewers join and leave streams
        /// </summary>
        /// <param name="first">Maximum number of objects to return. Maximum: 100. Default: 20</param>
        /// <param name="after">(Optional) Cursor for forward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="before">(Optional) Cursor for backward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="community_id">(Optional) Returns streams in a specified community ID. You can specify up to 100 IDs</param>
        /// <param name="game_id">(Optional) Returns streams broadcasting a specified game ID. You can specify up to 100 IDs</param>
        /// <param name="language">(Optional) Stream language. You can specify up to 100 languages</param>
        /// <param name="user_id">(Optional) Returns streams broadcast by one or more specified user IDs. You can specify up to 100 IDs</param>
        /// <param name="user_login">(Optional) Returns streams broadcast by one or more specified user login names. You can specify up to 100 names</param>
        /// <returns>An array of streams</returns>
        /// <exception cref="System.ArgumentException">Thrown if any of the optional string array parameters contains over 100 entries</exception>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Streams.StreamsList GetStreams(int first, string after, string before, string[] community_id, string[] game_id, string[] language,
            string[] user_id, string[] user_login)
        {
            Streams.StreamsList obj;

            first = Math.Min(100, Math.Max(1, first));

            if ((community_id != null && community_id.Length > 100) || (game_id != null && game_id.Length > 100) || (language != null && language.Length > 100)
                || (user_id != null && user_id.Length > 100) || (user_login != null && user_login.Length > 100))
            {
                throw new ArgumentException("Optional string array arguments can not have more than 100 entries");
            }

            try
            {
                string param = after != null && after.Length > 0 ? "after=" + after : before != null && before.Length > 0 ? "before=" + before : "";
                param += param.Length > 0 ? "&" : "";
                param += community_id != null && community_id.Length > 0 ? "community_id=" + string.Join("&community_id=", community_id) : "";
                param += param.Length > 0 ? "&" : "";
                param += game_id != null && game_id.Length > 0 ? "game_id=" + string.Join("&game_id=", game_id) : "";
                param += param.Length > 0 ? "&" : "";
                param += language != null && language.Length > 0 ? "language=" + string.Join("&language=", language) : "";
                param += param.Length > 0 ? "&" : "";
                param += user_id != null && user_id.Length > 0 ? "user_id=" + string.Join("&user_id=", user_id) : "";
                param += param.Length > 0 ? "&" : "";
                param += user_login != null && user_login.Length > 0 ? "user_login=" + string.Join("&user_login=", user_login) : "";
                param += param.Length > 0 ? "&" : "";
                param += "first=" + first;

                string data = this.GetData(Request_type.GET, TwitchAPIHelix.base_url + "streams?" + param, "", false, null);

                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Streams.StreamsList));

                MemoryStream ms = null;

                try
                {
                    ms = new MemoryStream(Encoding.UTF8.GetBytes(data))
                    {
                        Position = 0
                    };
                    obj = (Streams.StreamsList)js.ReadObject(ms);
                }
                finally
                {
                    if (ms != null)
                    {
                        ms.Dispose();
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }

            return obj;
        }

        /// <summary>
        /// Gets information about active streams. Streams are returned sorted by number of current viewers, in descending order.
        /// Across multiple pages of results, there may be duplicate or missing streams, as viewers join and leave streams
        /// </summary>
        /// <param name="first">Maximum number of objects to return. Maximum: 100. Default: 20</param>
        /// <param name="after">(Optional) Cursor for forward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="before">(Optional) Cursor for backward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="community_id">(Optional) Returns streams in a specified community ID. You can specify up to 100 IDs</param>
        /// <param name="game_id">(Optional) Returns streams broadcasting a specified game ID. You can specify up to 100 IDs</param>
        /// <param name="language">(Optional) Stream language. You can specify up to 100 languages</param>
        /// <param name="user_id">(Optional) Returns streams broadcast by one or more specified user IDs. You can specify up to 100 IDs</param>
        /// <param name="user_login">(Optional) Returns streams broadcast by one or more specified user login names. You can specify up to 100 names</param>
        /// <returns>An array of streams</returns>
        /// <exception cref="System.ArgumentException">Thrown if any of the optional string array parameters contains over 100 entries</exception>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Task<Streams.StreamsList> GetStreamsAsync(int first, string after, string before, string[] community_id, string[] game_id, string[] language,
            string[] user_id, string[] user_login) => Task.Run(() => this.GetStreams(first, after, before, community_id, game_id, language, user_id, user_login));

        /// <summary>
        /// Gets metadata information about active streams playing Overwatch or Hearthstone. Streams are sorted by number of current viewers, in descending order.
        /// Across multiple pages of results, there may be duplicate or missing streams, as viewers join and leave streams
        /// </summary>
        /// <param name="first">Maximum number of objects to return. Maximum: 100. Default: 20</param>
        /// <param name="after">(Optional) Cursor for forward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="before">(Optional) Cursor for backward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="community_id">(Optional) Returns streams in a specified community ID. You can specify up to 100 IDs</param>
        /// <param name="game_id">(Optional) Returns streams broadcasting a specified game ID. You can specify up to 100 IDs</param>
        /// <param name="language">(Optional) Stream language. You can specify up to 100 languages</param>
        /// <param name="user_id">(Optional) Returns streams broadcast by one or more specified user IDs. You can specify up to 100 IDs</param>
        /// <param name="user_login">(Optional) Returns streams broadcast by one or more specified user login names. You can specify up to 100 names</param>
        /// <returns>An array of streams</returns>
        /// <exception cref="System.ArgumentException">Thrown if any of the optional string array parameters contains over 100 entries</exception>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Streams.StreamsList GetStreamsMetadata(int first, string after, string before, string[] community_id, string[] game_id, string[] language,
            string[] user_id, string[] user_login)
        {
            Streams.StreamsList obj;

            first = Math.Min(100, Math.Max(1, first));

            if ((community_id != null && community_id.Length > 100) || (game_id != null && game_id.Length > 100) || (language != null && language.Length > 100)
                || (user_id != null && user_id.Length > 100) || (user_login != null && user_login.Length > 100))
            {
                throw new ArgumentException("Optional string array arguments can not have more than 100 entries");
            }

            try
            {
                string param = after != null && after.Length > 0 ? "after=" + after : before != null && before.Length > 0 ? "before=" + before : "";
                param += param.Length > 0 ? "&" : "";
                param += community_id != null && community_id.Length > 0 ? "community_id=" + string.Join("&community_id=", community_id) : "";
                param += param.Length > 0 ? "&" : "";
                param += game_id != null && game_id.Length > 0 ? "game_id=" + string.Join("&game_id=", game_id) : "";
                param += param.Length > 0 ? "&" : "";
                param += language != null && language.Length > 0 ? "language=" + string.Join("&language=", language) : "";
                param += param.Length > 0 ? "&" : "";
                param += user_id != null && user_id.Length > 0 ? "user_id=" + string.Join("&user_id=", user_id) : "";
                param += param.Length > 0 ? "&" : "";
                param += user_login != null && user_login.Length > 0 ? "user_login=" + string.Join("&user_login=", user_login) : "";
                param += param.Length > 0 ? "&" : "";
                param += "first=" + first;

                string data = this.GetData(Request_type.GET, TwitchAPIHelix.base_url + "streams?" + param, "", false, null);

                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Streams.StreamsList));

                MemoryStream ms = null;

                try
                {
                    ms = new MemoryStream(Encoding.UTF8.GetBytes(data))
                    {
                        Position = 0
                    };
                    obj = (Streams.StreamsList)js.ReadObject(ms);
                }
                finally
                {
                    if (ms != null)
                    {
                        ms.Dispose();
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }

            return obj;
        }

        /// <summary>
        /// Gets metadata information about active streams playing Overwatch or Hearthstone. Streams are sorted by number of current viewers, in descending order.
        /// Across multiple pages of results, there may be duplicate or missing streams, as viewers join and leave streams
        /// </summary>
        /// <param name="first">Maximum number of objects to return. Maximum: 100. Default: 20</param>
        /// <param name="after">(Optional) Cursor for forward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="before">(Optional) Cursor for backward pagination: tells the server where to start fetching the next set of results,
        /// in a multi-page response. The cursor value specified here is from the pagination response field of a prior query</param>
        /// <param name="community_id">(Optional) Returns streams in a specified community ID. You can specify up to 100 IDs</param>
        /// <param name="game_id">(Optional) Returns streams broadcasting a specified game ID. You can specify up to 100 IDs</param>
        /// <param name="language">(Optional) Stream language. You can specify up to 100 languages</param>
        /// <param name="user_id">(Optional) Returns streams broadcast by one or more specified user IDs. You can specify up to 100 IDs</param>
        /// <param name="user_login">(Optional) Returns streams broadcast by one or more specified user login names. You can specify up to 100 names</param>
        /// <returns>An array of streams</returns>
        /// <exception cref="System.ArgumentException">Thrown if any of the optional string array parameters contains over 100 entries</exception>
        /// <exception cref="Exceptions.AuthorizationRequiredException">Thrown if <see cref="TwitchAPIHelix.clientidOrOauth"/> is not set</exception>
        /// <exception cref="Exceptions.TwitchErrorException">Thrown if Twitch returns an error</exception>
        /// <exception cref="System.Net.WebException">Thrown if the HTTP request fails</exception>
        public Task<Streams.StreamsList> GetStreamsMetadataAsync(int first, string after, string before, string[] community_id, string[] game_id, string[] language,
            string[] user_id, string[] user_login) => Task.Run(() => this.GetStreamsMetadata(first, after, before, community_id, game_id, language, user_id, user_login));
    }
}