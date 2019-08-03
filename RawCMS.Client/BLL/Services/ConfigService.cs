﻿
//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Minà</author>
// <autogenerated>true</autogenerated>
//******************************************************************************using System;
using System;
using System.IO;
using RawCMS.Client.BLL.Interfaces;
using RawCMS.Client.BLL.Model;

namespace RawCMS.Client.BLL.Services
{
    public class ConfigService: IConfigService
    {
        private readonly ILoggerService _loggerService;

        public ConfigService(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public ConfigFile Load()
        {
            ConfigFile ConfigContent = new ConfigFile();

            _loggerService.Debug("get configuration file...");

            string filePath = Environment.GetEnvironmentVariable("RAWCMSCONFIG", EnvironmentVariableTarget.Process);


            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            _loggerService.Debug($"Config file: {filePath}");

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string data = sr.ReadToEnd();
                    ConfigContent = new ConfigFile(data);

                    _loggerService.Debug($"config loaded.");
                }
            }
            catch (Exception e)
            {
                _loggerService.Error("The file could not be read:", e);
            }
            return ConfigContent;
        }

        public ConfigFile Save(string filePath)
        {

            _loggerService.Debug("Save configiguration file...");

            _loggerService.Debug($"FilePath: {filePath}");

            try
            {
                using (StreamWriter outputFile = new StreamWriter(filePath))
                {
                    outputFile.Write(this.ToString());
                }
            }
            catch (Exception e)
            {
                _loggerService.Error("The file could not be writed:", e);
            }
            return Load();
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

}
