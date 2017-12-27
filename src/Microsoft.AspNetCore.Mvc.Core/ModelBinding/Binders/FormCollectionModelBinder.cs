// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation to bind form values to <see cref="IFormCollection"/>.
    /// </summary>
    public class FormCollectionModelBinder : IModelBinder
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FormCollectionModelBinder"/>.
        /// </summary>
        public FormCollectionModelBinder()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FormCollectionModelBinder"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public FormCollectionModelBinder(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// The <see cref="ILogger"/> used for logging in this binder.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc />
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            Logger.AttemptingToBindModel(bindingContext);

            object model;
            var request = bindingContext.HttpContext.Request;
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                model = form;
            }
            else
            {
                Logger.CannotBindToFilesCollectionDueToUnsupportedContentType(bindingContext);
                model = new EmptyFormCollection();
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            Logger.DoneAttemptingToBindModel(bindingContext);
        }

        private class EmptyFormCollection : IFormCollection
        {
            public StringValues this[string key] => StringValues.Empty;

            public int Count => 0;

            public IFormFileCollection Files => new EmptyFormFileCollection();

            public ICollection<string> Keys => new List<string>();

            public bool ContainsKey(string key)
            {
                return false;
            }

            public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
            {
                return Enumerable.Empty<KeyValuePair<string, StringValues>>().GetEnumerator();
            }

            public bool TryGetValue(string key, out StringValues value)
            {
                value = default(StringValues);
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class EmptyFormFileCollection : List<IFormFile>, IFormFileCollection
        {
            public IFormFile this[string name] => null;

            public IFormFile GetFile(string name) => null;

            IReadOnlyList<IFormFile> IFormFileCollection.GetFiles(string name) => null;
        }
    }
}