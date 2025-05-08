// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Samples;

MemoryStream stream = new();
await Simple.Deserialization(stream);
