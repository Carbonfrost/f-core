//
// Copyright 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Carbonfrost.Commons.Core.Runtime;

[assembly: Provides(typeof(IAdapterFactory))]
[assembly: Provides(typeof(IActivationFactory))]
[assembly: Provides(typeof(IActivationProvider))]
[assembly: Provides(typeof(StreamingSource))]

[assembly: Defines(AdapterRole.Builder)]
[assembly: Defines(AdapterRole.StreamingSource)]
[assembly: Defines(AdapterRole.ActivationProvider)]
[assembly: Defines(AdapterRole.Null)]
[assembly: Defines(AdapterRole.Template)]

[assembly: SharedRuntimeOptions(Optimizations = SharedRuntimeOptimizations.DisableTemplateScanning)]
