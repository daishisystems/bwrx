// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

/**
 * This application demonstrates how to perform basic operations on topics with
 * the Google Cloud Pub/Sub API.
 *
 * For more information, see the README.md under /pubsub and the documentation
 * at https://cloud.google.com/pubsub/docs.
 */

'use strict';

// [START pubsub_create_topic]
// Imports the Google Cloud client library
const { PubSub } = require('@google-cloud/pubsub');
const pubsub = new PubSub();
const topicName = 'projects/bwrx-dev/topics/input-sit-0';

exports.inputsit0 = async (req, res) => {
    res.status(200).send();
    if (!req.body) {
        res.status(500).send();
        return;
    }
    try {
        await publishEvents(topicName, req.body);
        res.status(202).send();
    } catch (error) {
        console.error(error);
        res.status(500).send();
    }
}

async function publishEvents(topicName, data) {
    // [START pubsub_publish]
    // [START pubsub_quickstart_publisher]
    const dataBuffer = Buffer.from(JSON.stringify(data), 'utf8');
    await pubsub.topic(topicName).publish(dataBuffer);
    // [END pubsub_publish]
    // [END pubsub_quickstart_publisher]
}