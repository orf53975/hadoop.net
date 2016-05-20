/* Generated By:JavaCC: Do not edit this line. ParseException.java Version 3.0 */
using Sharpen;

namespace org.apache.hadoop.record.compiler.generated
{
	/// <summary>This exception is thrown when parse errors are encountered.</summary>
	/// <remarks>
	/// This exception is thrown when parse errors are encountered.
	/// You can explicitly create objects of this exception type by
	/// calling the method generateParseException in the generated
	/// parser.
	/// You can modify this class to customize your error reporting
	/// mechanisms so long as you retain the public fields.
	/// </remarks>
	[System.Serializable]
	[System.ObsoleteAttribute(@"Replaced by <a href=""http://hadoop.apache.org/avro/"">Avro</a>."
		)]
	public class ParseException : System.Exception
	{
		/// <summary>
		/// This constructor is used by the method "generateParseException"
		/// in the generated parser.
		/// </summary>
		/// <remarks>
		/// This constructor is used by the method "generateParseException"
		/// in the generated parser.  Calling this constructor generates
		/// a new object of this type with the fields "currentToken",
		/// "expectedTokenSequences", and "tokenImage" set.  The boolean
		/// flag "specialConstructor" is also set to true to indicate that
		/// this constructor was used to create this object.
		/// This constructor calls its super class with the empty string
		/// to force the "toString" method of parent class "Throwable" to
		/// print the error message in the form:
		/// ParseException: <result of getMessage>
		/// </remarks>
		public ParseException(org.apache.hadoop.record.compiler.generated.Token currentTokenVal
			, int[][] expectedTokenSequencesVal, string[] tokenImageVal)
			: base(string.Empty)
		{
			specialConstructor = true;
			currentToken = currentTokenVal;
			expectedTokenSequences = expectedTokenSequencesVal;
			tokenImage = tokenImageVal;
		}

		/// <summary>
		/// The following constructors are for use by you for whatever
		/// purpose you can think of.
		/// </summary>
		/// <remarks>
		/// The following constructors are for use by you for whatever
		/// purpose you can think of.  Constructing the exception in this
		/// manner makes the exception behave in the normal way - i.e., as
		/// documented in the class "Throwable".  The fields "errorToken",
		/// "expectedTokenSequences", and "tokenImage" do not contain
		/// relevant information.  The JavaCC generated code does not use
		/// these constructors.
		/// </remarks>
		public ParseException()
			: base()
		{
			specialConstructor = false;
		}

		public ParseException(string message)
			: base(message)
		{
			specialConstructor = false;
		}

		/// <summary>
		/// This variable determines which constructor was used to create
		/// this object and thereby affects the semantics of the
		/// "getMessage" method (see below).
		/// </summary>
		protected internal bool specialConstructor;

		/// <summary>This is the last token that has been consumed successfully.</summary>
		/// <remarks>
		/// This is the last token that has been consumed successfully.  If
		/// this object has been created due to a parse error, the token
		/// followng this token will (therefore) be the first error token.
		/// </remarks>
		public org.apache.hadoop.record.compiler.generated.Token currentToken;

		/// <summary>Each entry in this array is an array of integers.</summary>
		/// <remarks>
		/// Each entry in this array is an array of integers.  Each array
		/// of integers represents a sequence of tokens (by their ordinal
		/// values) that is expected at this point of the parse.
		/// </remarks>
		public int[][] expectedTokenSequences;

		/// <summary>
		/// This is a reference to the "tokenImage" array of the generated
		/// parser within which the parse error occurred.
		/// </summary>
		/// <remarks>
		/// This is a reference to the "tokenImage" array of the generated
		/// parser within which the parse error occurred.  This array is
		/// defined in the generated ...Constants interface.
		/// </remarks>
		public string[] tokenImage;

		/// <summary>
		/// This method has the standard behavior when this object has been
		/// created using the standard constructors.
		/// </summary>
		/// <remarks>
		/// This method has the standard behavior when this object has been
		/// created using the standard constructors.  Otherwise, it uses
		/// "currentToken" and "expectedTokenSequences" to generate a parse
		/// error message and returns it.  If this object has been created
		/// due to a parse error, and you do not catch it (it gets thrown
		/// from the parser), then this method is called during the printing
		/// of the final stack trace, and hence the correct error message
		/// gets displayed.
		/// </remarks>
		public override string Message
		{
			get
			{
				if (!specialConstructor)
				{
					return base.Message;
				}
				System.Text.StringBuilder expected = new System.Text.StringBuilder();
				int maxSize = 0;
				for (int i = 0; i < expectedTokenSequences.Length; i++)
				{
					if (maxSize < expectedTokenSequences[i].Length)
					{
						maxSize = expectedTokenSequences[i].Length;
					}
					for (int j = 0; j < expectedTokenSequences[i].Length; j++)
					{
						expected.Append(tokenImage[expectedTokenSequences[i][j]]).Append(" ");
					}
					if (expectedTokenSequences[i][expectedTokenSequences[i].Length - 1] != 0)
					{
						expected.Append("...");
					}
					expected.Append(eol).Append("    ");
				}
				string retval = "Encountered \"";
				org.apache.hadoop.record.compiler.generated.Token tok = currentToken.next;
				for (int i_1 = 0; i_1 < maxSize; i_1++)
				{
					if (i_1 != 0)
					{
						retval += " ";
					}
					if (tok.kind == 0)
					{
						retval += tokenImage[0];
						break;
					}
					retval += add_escapes(tok.image);
					tok = tok.next;
				}
				retval += "\" at line " + currentToken.next.beginLine + ", column " + currentToken
					.next.beginColumn;
				retval += "." + eol;
				if (expectedTokenSequences.Length == 1)
				{
					retval += "Was expecting:" + eol + "    ";
				}
				else
				{
					retval += "Was expecting one of:" + eol + "    ";
				}
				retval += expected.ToString();
				return retval;
			}
		}

		/// <summary>The end of line string for this machine.</summary>
		protected internal string eol = Sharpen.Runtime.getProperty("line.separator", "\n"
			);

		/// <summary>
		/// Used to convert raw characters to their escaped version
		/// when these raw version cannot be used as part of an ASCII
		/// string literal.
		/// </summary>
		protected internal virtual string add_escapes(string str)
		{
			System.Text.StringBuilder retval = new System.Text.StringBuilder();
			char ch;
			for (int i = 0; i < str.Length; i++)
			{
				switch (str[i])
				{
					case 0:
					{
						continue;
					}

					case '\b':
					{
						retval.Append("\\b");
						continue;
					}

					case '\t':
					{
						retval.Append("\\t");
						continue;
					}

					case '\n':
					{
						retval.Append("\\n");
						continue;
					}

					case '\f':
					{
						retval.Append("\\f");
						continue;
					}

					case '\r':
					{
						retval.Append("\\r");
						continue;
					}

					case '\"':
					{
						retval.Append("\\\"");
						continue;
					}

					case '\'':
					{
						retval.Append("\\\'");
						continue;
					}

					case '\\':
					{
						retval.Append("\\\\");
						continue;
					}

					default:
					{
						if ((ch = str[i]) < unchecked((int)(0x20)) || ch > unchecked((int)(0x7e)))
						{
							string s = "0000" + int.toString(ch, 16);
							retval.Append("\\u" + Sharpen.Runtime.substring(s, s.Length - 4, s.Length));
						}
						else
						{
							retval.Append(ch);
						}
						continue;
					}
				}
			}
			return retval.ToString();
		}
	}
}