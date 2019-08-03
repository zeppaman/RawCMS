﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Minà</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RawCMS.Client.BLL.Core;
using RawCMS.Client.BLL.Interfaces;
using RawCMS.Client.BLL.Model;
using RestSharp;

namespace RawCMS.Client.BLL.Services
{
    public class RawCmsService:IRawCmsService
    {
        private readonly ILoggerService _loggerService;

        public RawCmsService(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public IRestResponse GetData(ListRequest req)
        {
            string url = $"{req.Url}/api/CRUD/{req.Collection}";

            _loggerService.Debug($"Service url: {url}");

            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest(Method.GET)
            {
                //request headers
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Content-Type", "application/json");

            //add parameters and token to request
            request.Parameters.Clear();
            if (!string.IsNullOrEmpty(req.RawQuery))
            {
                request.AddParameter("rawQuery", req.RawQuery);
            }
            request.AddParameter("pageNumber", req.PageNumber);
            request.AddParameter("pageSize", req.PageSize);

            //request.AddParameter("Authorization", "Bearer " + req.Token, ParameterType.HttpHeader);

            // log request
            var fullUrl = client.BuildUri(request);
            _loggerService.Debug($"request URI: {fullUrl}");




            //make the API request and get a response
            IRestResponse response = client.Execute(request);

            return response;
        }

        public  void ElaborateQueue(Dictionary<string, List<string>> listFile, ConfigFile config, bool pretty)
        {
            int totalfile = listFile.Sum(x => x.Value.Count);
            int partialOfTotal = 0;

            foreach (KeyValuePair<string, List<string>> c in listFile)
            {
                int progress = 0;

                foreach (string item in c.Value)
                {
                    _loggerService.Info($"Processing file {++progress} of {c.Value.Count} in collection: {c.Key}");

                    string contentFile = File.ReadAllText(item);

                    _loggerService.Request(contentFile);

                    IRestResponse responseRawCMS = CreateElement(new CreateRequest
                    {
                        Collection = c.Key,
                        Data = contentFile,
                        Token = config.Token,
                        Url = config.ServerUrl
                    });

                    _loggerService.Debug($"RawCMS response code: {responseRawCMS.StatusCode}");

                    if (!responseRawCMS.IsSuccessful)
                    {
                        //log.Error($"Error occurred: \n{responseRawCMS.Content}");
                        _loggerService.Error($"Error: {responseRawCMS.ErrorMessage}");
                    }
                    else
                    {
                        _loggerService.Response(responseRawCMS.Content);
                    }

                    //switch (responseRawCMS.ResponseStatus)
                    //{
                    //    case RestSharp.ResponseStatus.Completed:
                    //        log.Response(responseRawCMS.Content);

                    //        break;

                    //    case RestSharp.ResponseStatus.None:
                    //    case RestSharp.ResponseStatus.Error:
                    //    case RestSharp.ResponseStatus.TimedOut:
                    //    case RestSharp.ResponseStatus.Aborted:

                    //    default:
                    //        log.Error($"Error response: {responseRawCMS.ErrorMessage}");
                    //        break;
                    //}

                    _loggerService.Info($"File processed\n\tCollection progress: {progress} of {c.Value.Count}\n\tTotal progress: {++partialOfTotal} of {totalfile}\n\tFile: {item}\n\tCollection: {c.Key}");
                }
            }
        }

        public  IRestResponse CreateElement(CreateRequest req)
        {
            string url = $"{req.Url}/api/CRUD/{req.Collection}";

            _loggerService.Debug($"Server URL: {url}");
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest(Method.POST)
            {
                //request headers
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Content-Type", "application/json");

            //add parameters and token to request
            request.Parameters.Clear();
            request.AddParameter("application/json", req.Data, ParameterType.RequestBody);
            request.AddParameter("Authorization", "Bearer " + req.Token, ParameterType.HttpHeader);

            //make the API request and get a response
            IRestResponse response = client.Execute(request);

            return response;
        }

        public void ProcessDirectory(bool recursive, Dictionary<string, List<string>> fileList, string targetDirectory, string collection = null)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                if (!fileList.ContainsKey(collection))
                {
                    fileList.Add(collection, new List<string>());
                }
                fileList[collection].Add(fileName);
            }

            if (recursive)
            {
                // Recurse into subdirectories of this directory.
                // this is the main level, so take care of name
                // of folder, thus is name of collection...

                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    ProcessDirectory(recursive, fileList, subdirectory, collection);
                }
            }
        }

        public int CheckJSON(string filePath)
        {
            string content = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content))
            {
                return 1;
            }

            try
            {
                JObject obj = JObject.Parse(content);
            }
            catch (JsonReaderException jex)
            {
                //Exception in parsing json
                _loggerService.Error(jex.Message);
                return 2;
            }
            catch (Exception ex) //some other exception
            {
                _loggerService.Error(ex.ToString());
                return 2;
            }
            return 0;
        }

        public string FixUrl(string serverUrl)
        {
            // check if url finish with /
            // remove all / at the end

            if (!string.IsNullOrEmpty(serverUrl))
            {
                while (serverUrl[serverUrl.Length - 1] == '/')
                {
                    serverUrl = serverUrl.Substring(0, serverUrl.Length - 1);
                }
            }
            return serverUrl;
        }

        public bool Ping(string url)
        {
            // TODO fix ping
            return true;

            //Ping pingSender = new Ping();
            //PingOptions options = new PingOptions();

            //// Use the default Ttl value which is 128,
            //// but change the fragmentation behavior.
            //options.DontFragment = true;

            //// Create a buffer of 32 bytes of data to be transmitted.
            //string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            //byte[] buffer = Encoding.ASCII.GetBytes(data);

            //int timeout = 120;

            //try
            //{
            //    PingReply reply = pingSender.Send(url, timeout);

            //    if (reply.Status == IPStatus.Success)
            //    {
            //        log.Debug($"Address: {reply.Address.ToString()}");
            //        log.Debug($"RoundTrip time: {reply.RoundtripTime}");
            //        log.Debug($"Time to live: {reply.Options.Ttl}");
            //        log.Debug($"Don't fragment: {reply.Options.DontFragment}");
            //        log.Debug($"Buffer size: { reply.Buffer.Length}");

            //    }
            //    else
            //    {
            //        log.Error($"unable to ping host:\n\t{url}");
            //        return false;
            //    }
            //}
            //catch(Exception e )
            //{
            //    log.Error($"error on ping url: {url}.\nerror: {e.Message}");
            //    return false;
            //}

            //return true;

        }

    }
}
