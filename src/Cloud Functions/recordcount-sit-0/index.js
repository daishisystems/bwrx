/**
 * Copyright 2018, Google LLC.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

'use strict';

// [START functions_sql_mysql]
const mysql = require('mysql');
// [END functions_sql_mysql]

/**
 * TODO(developer): specify SQL connection details
 */
const connectionName = 'bwrx-dev:europe-west2:ipaddress-lists-0';
const dbUser = 'root';
const dbName = 'ipaddress_lists_0';

// [END functions_sql_mysql]

// [START functions_sql_mysql]
const mysqlConfig = {
    connectionLimit: 1,
    user: dbUser,
    database: dbName,
};
mysqlConfig.socketPath = `/cloudsql/${connectionName}`;

// Connection pools reuse connections between invocations,
// and handle dropped or expired connections automatically.
let mysqlPool;

exports.getrecordcountsit0 = (req, res) => {
    res.status(200).send();
    // Return if missing parameters
    if (!req.query) {
        res.status(500).send();
        return;
    }
    if (!req.query.tablename) {
        res.status(500).send();
        return;
    }
    // Initialize the pool lazily, in case SQL access isn't needed for this
    // GCF instance. Doing so minimizes the number of active SQL connections,
    // which helps keep your GCF instances under SQL connection limits.
    if (!mysqlPool) {
        mysqlPool = mysql.createPool(mysqlConfig);
    }

    const tableName = req.query.tablename;

    mysqlPool.query(`select count(*) as Total from ` + tableName + `;`, (err, results) => {
        if (err) {
            console.error(err);
            res.status(500).send(err);
        } else {
            res.set('Content-Type', 'application/json');
            res.send(JSON.stringify(results));
        }
    });

    // Close any SQL resources that were declared inside this function.
    // Keep any declared in global scope (e.g. mysqlPool) for later reuse.
};
// [END functions_sql_mysql]