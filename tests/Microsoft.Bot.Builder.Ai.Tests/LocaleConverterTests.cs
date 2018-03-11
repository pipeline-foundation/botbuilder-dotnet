﻿using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Storage;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class LocaleConverterTests
    {
        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverter_ConvertFromFrench()
        {
            LocaleConverter localeConverter = new LocaleConverter();

            var convertedMessage = await localeConverter.Convert("Set a meeting on 30/9/2017", "fr-fr", "en-us");
            Assert.IsNotNull(convertedMessage);
            Assert.AreEqual("Set a meeting on 09/30/2017", convertedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverter_ConvertToChinese()
        {
            LocaleConverter localeConverter = new LocaleConverter();

            var convertedMessage = await localeConverter.Convert("Book me a plane ticket for France on 12/25/2018", "en-us", "zh-cn");
            Assert.IsNotNull(convertedMessage);
            Assert.AreEqual("Book me a plane ticket for France on 2018-12-25", convertedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverter_InvalidFromLocale()
        {
            LocaleConverter localeConverter = new LocaleConverter();

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await localeConverter.Convert("Book me a plane ticket for France on 12/25/2018", "na-na", "en-us")); 
        }   

    [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverter_InvalidToLocale()
        {
            LocaleConverter localeConverter = new LocaleConverter();

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await localeConverter.Convert("Book me a plane ticket for France on 12/25/2018", "en-us", "na-na")); 
        }
    }
}
