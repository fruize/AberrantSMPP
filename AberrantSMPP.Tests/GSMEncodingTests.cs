﻿using System;
using System.Collections.Generic;
using System.Text;

using NUnit;
using NUnit.Framework;
using NUnit.Framework.Constraints;

using AberrantSMPP.Utility;

namespace AberrantSMPP.Tests
{
	[TestFixture]
	public class GSMEncodingTests
	{
		private const string _simpleString = @"@1234567890abcdefghijklmnopqrstxyzABCDEFGHIJKLMNOPQRSTXYZ";
		private byte[] _simpleBytes = new byte[] {
			0x00, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 
			0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 	0x74, 0x78, 0x79, 0x7a, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 
			0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 	0x51, 0x52, 0x53, 0x54, 0x58, 0x59, 0x5a 
		};
		private const string _extendedString = @"@£$¥Øø!#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNÑOPQRSTXYZabcdefghijklmnñopqrstxyzàèìòùéÄÖÜ§äöü^{}\[~]|€";
		private byte[] _extendedBytes = new byte[] {
			0x00, 0x01, 0x02, 0x03, 0x0b, 0x0c, 0x21, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
			0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 
			0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x5d, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x58, 0x59, 
			0x5a, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x7d, 0x6f, 0x70, 0x71, 0x72, 
			0x73, 0x74, 0x78, 0x79, 0x7a, 0x7f, 0x04, 0x07, 0x08, 0x06, 0x05, 0x5B, 0x5C, 0x5E, 0x5F, 0x7B, 0x7C, 0x7E, 
			
			0x1b, 0x14, 0x1b, 0x28, 0x1b, 0x29, 0x1b, 0x2f, 0x1b, 0x3c, 0x1b, 0x3d, 0x1b, 0x3e, 0x1b, 0x40, 0x1b, 0x65
		};

		private const string _invalidGsmString = "Unsupported º";
		private byte[] _invalidGsmBytes = new byte[] {
			0x55, 0x6e, 0x73, 0x75, 0x70, 0x70, 0x6f, 0x72, 0x74, 0x65, 0x64, 0x20, 0x3f
		};

		private GSMEncoding _encoding = null;

		[SetUp]
		public void SetUp()
		{
			_encoding = new GSMEncoding();
		}

		[Test]
		public void Encode_Simple_String()
		{
			var bytes = _encoding.GetBytes(_simpleString);
			CollectionAssert.AreEqual(_simpleBytes, bytes);
		}

		[Test]
		public void Decode_Simple_String()
		{
			var str = _encoding.GetString(_simpleBytes);
			Assert.AreEqual(_simpleString, str);
		}

		[Test]
		public void Encode_Extended_String()
		{
			var bytes = _encoding.GetBytes(_extendedString);
			CollectionAssert.AreEqual(_extendedBytes, bytes);
		}

		[Test]
		public void Decode_Extended_String()
		{
			var str = _encoding.GetString(_extendedBytes);
			Assert.AreEqual(_extendedString, str);
		}

		[Test]
		public void Encode_Invalid_String()
		{
			var bytes = _encoding.GetBytes(_invalidGsmString);
			CollectionAssert.AreEqual(_invalidGsmBytes, bytes);
		}

		[Test]
		public void Encoding_Invalid_Char_Throws()
		{
			Assert.Throws<EncoderFallbackException>(() =>
				new GSMEncoding(true).GetBytes(new char[] { '\x03a2' })
			);
		}

		[Test]
		public void Decoding_Invalid_Char_Throws()
		{
			Assert.Throws<EncoderFallbackException>(() =>
				new GSMEncoding(true).GetString(new byte[] { 0x80 })
			);
		}

		[Test]
		public void Encoding_Using_BestFit()
		{
			var bytes = new GSMEncoding(true, true).GetBytes("ÁÉÍÓÚ ÀÈÌÒÙ áéíóú €");
			var valid = new byte[] { 
				0x7f, 0x1f, 0x07, 0x08, 0x06, 0x20, 
				0x7f, 0x1f, 0x07, 0x08, 0x06, 0x20, 
				0x7f, 0x05, 0x07, 0x08, 0x06, 0x20, 
				0x1b, 0x65
			};
			CollectionAssert.AreEqual(valid, bytes);
		}

		[Test]
		public void Encoding_Invalid_Char_Using_BestFit_Throws_1()
		{
			Assert.Throws<EncoderFallbackException>(() =>
				new GSMEncoding(true, true).GetBytes(new char[] { 'º' })
			);
		}

		[Test]
		public void Encoding_Invalid_Char_Using_BestFit_Throws_2()
		{
			Assert.Throws<EncoderFallbackException>(() =>
				new GSMEncoding(true, true).GetBytes(new char[] { '\x03a2' })
			);
		}

		[Test]
		public void Encoding_Invalid_Char_Using_BestFit_Replaces_Character()
		{
			var bytes = new GSMEncoding(true, false).GetBytes(new char[] { 'º' });
			CollectionAssert.AreEqual(new byte[] { 0x3f }, bytes);
		}

	}
}
