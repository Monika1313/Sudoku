﻿global using System;
global using System.CommandLine;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Reactive.Linq;
global using System.Resources;
global using System.Runtime.CompilerServices;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading.Tasks;
global using Flurl.Http;
global using Mirai.Net.Data.Events.Concretes.Group;
global using Mirai.Net.Data.Events.Concretes.Request;
global using Mirai.Net.Data.Exceptions;
global using Mirai.Net.Data.Messages.Receivers;
global using Mirai.Net.Data.Shared;
global using Mirai.Net.Sessions;
global using Mirai.Net.Sessions.Http.Managers;
global using Mirai.Net.Utils.Scaffolds;
global using Sudoku.Communication.Qicq.Models;
global using Sudoku.Communication.Qicq.Permission;
global using Sudoku.Communication.Qicq.Strings;
global using static System.Math;
global using static Sudoku.Resources.MergedResources;
global using SpecialFolder = System.Environment.SpecialFolder;
global using File = System.IO.File;
