﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crunch.Core;
using Crunch.Infrastructure.Database;
using Crunch.Infrastructure.DataSources;

namespace Crunch.UseCases
{
    static class ImportGroupsUseCase
    {
        /// <summary>
        /// Data source API instance
        /// </summary>
        private static readonly DataSourceAPI _dataSource = new DataSourceAPI();

        /// <summary>
        /// Database API instance
        /// </summary>
        private static readonly DatabaseAPI _database = new DatabaseAPI();

        public static void ImportGroups()
        {
            var groupsData = _dataSource.GetGroupsData();
            _database.SaveGroups(groupsData);
        }
    }

 }
