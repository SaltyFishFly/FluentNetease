﻿using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace FluentNetease.Classes
{
    class Network
    {
        /// <summary>
        /// 登录(目前只支持手机)
        /// </summary>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        /// <returns>(Code, JObject) 返回代码和JSON对象</returns>
        public static async Task<(int, JObject)> LoginAsync(string account, string password)
        {
            var Parameters = new Dictionary<string, object>
                {
                    { "phone", account },
                    { "password", password }
                };
            var (IsSuccess, RequestResult) = await App.CLOUD_MUSIC_API.RequestAsync(CloudMusicApiProviders.LoginCellphone, Parameters);
            if (IsSuccess)
            {
                var RequestCode = RequestResult["code"].Value<int>();
                if (RequestCode == 200)
                {
                    return (RequestCode, RequestResult);
                }
                return (RequestCode, null);
            }
            return (404, null);
        }

        /// <summary>
        /// 获取日推
        /// </summary>
        /// <returns></returns>
        public static async Task<LinkedList<Playlist>> GetDailyPlaylistAsync()
        {
            var RequestResult = (await App.CLOUD_MUSIC_API.RequestAsync(CloudMusicApiProviders.RecommendResource)).Item2;
            var Result = new LinkedList<Playlist> { };
            if (RequestResult["code"].Value<int>() == 200)
            {
                foreach (var Item in RequestResult["recommend"])
                {
                    var Playlist = new Playlist(Item["id"].ToString(), Item["name"].ToString(), Item["picUrl"].ToString());
                    Result.AddLast(Playlist);
                }
            }
            return Result;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keywords">搜索词</param>
        /// <param name="type">搜索类型</param>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public static async Task<(LinkedList<Song> SearchResults, int CurrentPage)> SearchAsync(SearchRequest Request)
        {
            var (IsSuccess, RequestResult) = await App.CLOUD_MUSIC_API.RequestAsync(CloudMusicApiProviders.Search, Request.ToDictionary());
            if (IsSuccess && RequestResult["code"].Value<int>() == 200)
            {
                var Result = new LinkedList<Song>();
                foreach (JToken JsonSearchResult in RequestResult["result"]["songs"])
                {
                    var SearchResult = new Song(JsonSearchResult, Song.SongSource.Search);
                    Result.AddLast(SearchResult);
                }
                //计算页数
                int Page = (int)Math.Ceiling(RequestResult["result"]["songCount"].Value<double>() / Request.Limit);
                return (Result, Page);
            }
            return (null, 0);
        }

        /// <summary>
        /// 获取音乐播放地址
        /// </summary>
        /// <param name="musicId"></param>
        /// <returns></returns>
        public static async Task<(bool IsSuccess, MediaPlaybackItem Result)> GetMusicUrlAsync(string musicId)
        {
            var Parameters = new Dictionary<string, object> { { "id", musicId } };
            var (IsSuccess, RequestResult) = await App.CLOUD_MUSIC_API.RequestAsync(CloudMusicApiProviders.SongUrl, Parameters);
            if (IsSuccess && RequestResult["code"].Value<int>() == 200)
            {
                return (true, new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(RequestResult["data"].First["url"].ToString()))));
            }
            return (false, null);
        }

        public static async Task<(bool IsSuccess, MediaPlaybackList Result)> GetMusicUrlAsync(IList<Song> musicIdList)
        {
            var Result = new MediaPlaybackList();
            foreach (var Item in musicIdList)
            {
                var (IsSuccess, RequestResult) = await GetMusicUrlAsync(Item.Music.ID);
                if (IsSuccess)
                {
                    Result.Items.Add(RequestResult);
                }
            }
            if (Result.Items.Count > 0)
            {
                return (true, Result);
            }
            return (false, null);
        }

        /// <summary>
        /// 获取歌单的封面和所有歌曲
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static async Task<(bool IsSuccess, JToken PlaylistInfo , LinkedList<Song> SongList)> GetPlaylistDetailAsync(string playlistID)
        {
            var Parameters = new Dictionary<string, object> { { "id", playlistID } };
            var (IsSuccess, RequestResult) = await App.CLOUD_MUSIC_API.RequestAsync(CloudMusicApiProviders.PlaylistDetail, Parameters);
            if (IsSuccess && RequestResult["code"].Value<int>() == 200)
            {
                StringBuilder MusicIds = new StringBuilder();
                foreach (var Item in RequestResult["playlist"]["trackIds"])
                {
                    MusicIds.Append(Item["id"].ToString());
                    MusicIds.Append(",");
                }
                MusicIds.Remove(MusicIds.Length - 1, 1);
                var Result = new LinkedList<Song>();
                var Parameters2 = new Dictionary<string, object> { { "ids", MusicIds.ToString() } };
                var (IsSuccess2, RequestResult2) = await App.CLOUD_MUSIC_API.RequestAsync(CloudMusicApiProviders.SongDetail, Parameters2);
                if (IsSuccess2 && RequestResult2["code"].Value<int>() == 200)
                {
                    foreach (var Item in RequestResult2["songs"])
                    {
                        Result.AddLast(new Song(Item, Song.SongSource.PlayList));
                    }
                    return (true, RequestResult["playlist"], Result);
                }
                return (false, null, null);
            }
            return (false, null, null);
        }

        /// <summary>
        /// 获取专辑信息
        /// </summary>
        /// <param name="albumID"></param>
        /// <returns></returns>
        public static async Task<(bool IsSuccess, JToken AlbumInfo, LinkedList<Song> Result)> GetAlbumDetailAsync(string albumID)
        {
            var Parameters = new Dictionary<string, object>
            {
                { "id", albumID }
            };
            var (IsSuccess, RequestResult) = await App.CLOUD_MUSIC_API.RequestAsync(CloudMusicApiProviders.Album, Parameters);
            if (IsSuccess && RequestResult["code"].Value<int>() == 200)
            {
                var Result = new LinkedList<Song>();
                foreach (var Item in RequestResult["songs"])
                {
                    Result.AddLast(new Song(Item, Song.SongSource.PlayList));
                }
                return (true, RequestResult["album"], Result);
            }
            return (false, null, null);
        }
    }
}