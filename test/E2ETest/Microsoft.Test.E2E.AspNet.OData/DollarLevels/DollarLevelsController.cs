// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.DollarLevels
{
    public class DLManagersController : TestODataController
    {
        public DLManagersController()
        {
            if (null == _DLManagers)
            {
                InitDLManagers();
            }
        }

        /// <summary>
        /// static so that the data is shared among requests.
        /// </summary>
        private static List<DLManager> _DLManagers = null;

        private static void InitDLManagers()
        {
            _DLManagers = Enumerable.Range(1, 10).Select(i =>
                        new DLManager
                        {
                            ID = i,
                            Name = "Name" + i,
                        }).ToList();

            for (int i = 0; i < 9; i++)
            {
                _DLManagers[i].Manager = _DLManagers[i + 1];
                _DLManagers[i + 1].DirectReports = new List<DLManager> { _DLManagers[i] };
            }
        }

        public ITestActionResult Get(ODataQueryOptions<DLManager> queryOptions)
        {
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.MaxExpansionDepth = 1;

            try
            {
                queryOptions.Validate(settings);
            }
            catch (ODataException e)
            {
                var responseMessage = String.Format("The query specified in the URI is not valid. {0}", e.Message);
                return BadRequest(responseMessage);
            }

            var managers = queryOptions.ApplyTo(_DLManagers.AsQueryable()).AsQueryable();
            return Ok(managers, managers.GetType());
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public ITestActionResult Get(int key)
        {
            return Ok(_DLManagers.Single(e => e.ID == key));

        }

        private ITestActionResult Ok(object content, Type type)
        {
            var resultType = typeof(TestOkObjectResult<>).MakeGenericType(type);
            return Activator.CreateInstance(resultType, content, this) as ITestActionResult;
        }
    }

    public class DLEmployeesController : TestODataController
    {
        public DLEmployeesController()
        {
            if (null == _DLEmployees)
            {
                InitDLEmployees();
            }
        }

        private static List<DLEmployee> _DLEmployees = null;

        private static void InitDLEmployees()
        {
            _DLEmployees = Enumerable.Range(1, 5).Select(i =>
                        new DLEmployee
                        {
                            ID = i,
                        }).ToList();

            for (int i = 0; i < 4; i++)
            {
                _DLEmployees[i].Friend = _DLEmployees[i + 1];
            }
        }

        public ITestActionResult Get(ODataQueryOptions<DLEmployee> queryOptions)
        {
            if (queryOptions.SelectExpand != null)
            {
                queryOptions.SelectExpand.LevelsMaxLiteralExpansionDepth = 2;
            }

            ODataValidationSettings settings = new ODataValidationSettings();
            settings.MaxExpansionDepth = 4;

            try
            {
                queryOptions.Validate(settings);
            }
            catch (ODataException e)
            {
                var responseMessage = String.Format("The query specified in the URI is not valid. {0}", e.Message);
                return BadRequest(responseMessage);
            }

            var employees = queryOptions.ApplyTo(_DLEmployees.AsQueryable()).AsQueryable();
            return Ok(employees, employees.GetType());
        }

        public ITestActionResult Get(int key, ODataQueryOptions<DLEmployee> queryOptions)
        {
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.MaxExpansionDepth = 3;

            try
            {
                queryOptions.Validate(settings);
            }
            catch (ODataException e)
            {
                var responseMessage = String.Format("The query specified in the URI is not valid. {0}", e.Message);
                return BadRequest(responseMessage);
            }

            var employee = _DLEmployees.Single(e => e.ID == key);
            var appliedEmployee = queryOptions.ApplyTo(employee, new ODataQuerySettings());
            return Ok(appliedEmployee, appliedEmployee.GetType());
        }

        private ITestActionResult Ok(object content, Type type)
        {
            var resultType = typeof(TestOkObjectResult<>).MakeGenericType(type);
            return Activator.CreateInstance(resultType, content, this) as ITestActionResult;
        }
    }
}
