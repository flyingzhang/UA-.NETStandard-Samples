/* ========================================================================
 * Copyright (c) 2005-2019 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Threading;
using System.Reflection;
using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Com;

namespace Opc.Ua.Com.Client
{
    /// <summary>
    /// An interface to an object that parses item ids.
    /// </summary>
    public interface IItemIdParser
    {
        /// <summary>
        /// Parses the specified item id.
        /// </summary>
        /// <param name="server">The COM server that provided the item id.</param>
        /// <param name="configuration">The COM wrapper configuration.</param>
        /// <param name="itemId">The item id to parse.</param>
        /// <param name="browseName">The name of the item.</param>
        /// <returns>True if the item id could be parsed.</returns>
        bool Parse(
            ComObject server,
            ComClientConfiguration configuration,
            string itemId,
            out string browseName);
    }

    /// <summary>
    /// The default item id parser that uses the settings in the configuration.
    /// </summary>
    public class ComItemIdParser : IItemIdParser
    {
        #region IItemIdParser Members
        /// <summary>
        /// Parses the specified item id.
        /// </summary>
        /// <param name="server">The COM server that provided the item id.</param>
        /// <param name="configuration">The COM wrapper configuration.</param>
        /// <param name="itemId">The item id to parse.</param>
        /// <param name="browseName">The name of the item.</param>
        /// <returns>True if the item id could be parsed.</returns>
        public bool Parse(ComObject server, ComClientConfiguration configuration, string itemId, out string browseName)
        {
            browseName = null;

            if (configuration == null || itemId == null)
            {
                return false;
            }

            if (String.IsNullOrEmpty(configuration.SeperatorChars))
            {                
                return false;
            }

            for (int ii = 0; ii < configuration.SeperatorChars.Length; ii++)
            {
                int index = itemId.LastIndexOf(configuration.SeperatorChars[ii]);

                if (index >= 0)
                {
                    browseName = itemId.Substring(index + 1);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }

    /// <summary>
    /// a patched item id parser that uses the settings in the configuration support a special PATCH
    /// </summary>
    public class ComDaItemIdParser : ComItemIdParser, IItemIdParser
    {
        #region IItemIdParser Members
        /// <summary>
        /// Parses the specified item id.
        /// </summary>
        /// <param name="server">The COM server that provided the item id.</param>
        /// <param name="configuration">The COM wrapper configuration.</param>
        /// <param name="itemId">The item id to parse.</param>
        /// <param name="browseName">The name of the item.</param>
        /// <returns>True if the item id could be parsed.</returns>
        public new bool Parse(ComObject server, ComClientConfiguration configuration, string itemId, out string browseName)
        {
            if (base.Parse(server, configuration, itemId, out browseName))
            {
                return true;
            }
            ComDaClientConfiguration dac = (ComDaClientConfiguration)configuration;

            if (dac != null && itemId != null)
            {
                // This is a PATCH rather than a soluation
                // Here we suppose any Items categoried into Group & Single Item.
                // Group Items should start with GroupPrefix and not contain ItemSeparator
                // Single Items can be either:
                //     a) not started with GroupPrefix
                //     b) started with GroupPrefix and contains ItemSeparator.
                // When there is no explicit group prefix, this patch not working.
                if (dac.GroupPrefix.Length > 0)
                {
                    if (itemId.StartsWith(dac.GroupPrefix))
                    {
                        if (dac.ItemSeparator.Length > 0)
                        {
                            int lastPos = itemId.LastIndexOf(dac.ItemSeparator);
                            if (lastPos >= 0)
                            {
                                browseName = itemId.Substring(lastPos + 1);
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        if (dac.ItemSeparator.Length > 0)
                        {
                            browseName = itemId.Substring(itemId.LastIndexOf(dac.ItemSeparator) + 1);
                        }
                        else
                        {
                            browseName = itemId;
                        }
                        return true;
                        
                    }
                }
            }
            return false;
        }
        #endregion
    }

}
