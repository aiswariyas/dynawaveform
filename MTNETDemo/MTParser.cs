﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTNETDemo
{
    public class MTParser
    {
        private const string hexDigits = "0123456789ABCDEF";

        public static List<Dictionary<string, string>> parseEMVData(byte[] data, bool hasSizeHeader, String stringPadRight)
        {
            List<Dictionary<String, String>> fillMaps = new List<Dictionary<string, string>>();
		
    	    if (data != null)
    	    {   		
    		    int dataLen = data.Length;    		    		
    		
    		    if (dataLen >= 2)
    		    {
    			    int tlvLen = data.Length;
    			    byte[] tlvData = data;
    			
    			    if (hasSizeHeader)
    			    {
    				    tlvLen = (int) ((data[0] & 0x000000FF) << 8) + (int) (data[1] & 0x000000FF) ;
	    		
    				    //tlvData = Arrays.copyOfRange(data, 2, tlvLen + 2);
    				    tlvData = new byte[tlvLen];
					    Array.Copy(data, 2, tlvData, 0, tlvLen);
    			    }
    			
	    		    if (tlvData != null)
				    {
    				    int iTLV;
    				    int iTag;
    				    int iLen;
    				    bool bTag;
    				    bool bMoreTagBytes;
    				    bool bConstructedTag;
    				    byte byteValue;
    				    int lengthValue;
  				    		
    				    byte[] tagBytes = null;
    				
                        const byte MoreTagBytesFlag1 = (byte)0x1F;
                        const byte MoreTagBytesFlag2 = (byte)0x80;
    				
    				    const byte ConstructedFlag 		= (byte) 0x20;

    				    const byte MoreLengthFlag 		= (byte) 0x80;
    				    const byte OneByteLengthMask 	= (byte) 0x7F;

                        byte[] TagBuffer = new byte[50];					

    				    bTag = true;    				
    				    iTLV = 0;    				
    				
    				    while (iTLV < tlvData.Length)
    				    {
    					    byteValue = tlvData[iTLV];

    					    if (bTag)
    					    {
    						    // Get Tag
    						    iTag = 0;    							
    						    bMoreTagBytes = true;
    							
							    while (bMoreTagBytes && (iTLV < tlvData.Length))
    						    {
								    byteValue = tlvData[iTLV];
    							    iTLV++;

    							    TagBuffer[iTag] = byteValue;

                                    if (iTag == 0)
                                    {
                                        bMoreTagBytes = ((byteValue & MoreTagBytesFlag1) == MoreTagBytesFlag1);
                                    }
                                    else
                                    {
                                        bMoreTagBytes = ((byteValue & MoreTagBytesFlag2) == MoreTagBytesFlag2);
                                    }

    							    iTag++;
/*    							
    		    				    if ((byteValue & SpecialTagFlag) == SpecialTagFlag)
    		    				    {
    		    					    bMoreTagBytes = false;
    		    				    }
    		    				    else
    		    				    { 
    		    					    bMoreTagBytes = ((byteValue & MoreTagBytesFlag) == MoreTagBytesFlag);
    		    				    }
 */ 
    						    }

							    tagBytes = new byte[iTag];
							    Array.Copy(TagBuffer, 0, tagBytes, 0, iTag);
							
							    bTag = false;
    					    }
    					    else
    					    {
    						    // Get Length
    	    				    lengthValue = 0;
    	    				
							    if ((byteValue & MoreLengthFlag) == MoreLengthFlag)
							    {
	    						    int nLengthBytes = (int) (byteValue & OneByteLengthMask);
	    						
    							    iTLV++;
	    						    iLen = 0;
	    						
								    while ((iLen < nLengthBytes) && (iTLV < tlvData.Length))
								    {
									    byteValue = tlvData[iTLV];
        							    iTLV++;
						    		    lengthValue = (int) ((lengthValue & 0x000000FF) << 8) + (int) (byteValue & 0x000000FF);
        							    iLen++;
								    }								
							    }
							    else
							    {
								    lengthValue = (int) (byteValue & OneByteLengthMask);
    							    iTLV++;
							    }
						
							    if (tagBytes != null)
							    {
								    int tagBytesLen = tagBytes.Length;
								    int tagByte = tagBytes[0];
								
			    				    bConstructedTag = ((tagByte & ConstructedFlag) == ConstructedFlag);

                                    if (bConstructedTag) 
								    {
									    // Constructed
                                        Dictionary<string, string> map = new Dictionary<string, string>();
                                        map.Add("tag", getHexString(tagBytes));
                                        map.Add("len", "" + lengthValue);
                                        map.Add("value", "[Container]");
                                        fillMaps.Add(map);
								    }
								    else
								    {
									    // Primitive									
									    int endIndex = iTLV + lengthValue;
									
									    if (endIndex > tlvData.Length)
										    endIndex =  tlvData.Length;
									
									    byte[] valueBytes = null;
									    int len = endIndex - iTLV;
									    if (len > 0)
									    {
										    valueBytes = new byte[len];
										    Array.Copy(tlvData, iTLV, valueBytes, 0, len);
									    }

                                        Dictionary<string, string> map = new Dictionary<string, string>();
                                        map.Add("tag", getHexString(tagBytes));
                                        map.Add("len", "" + lengthValue);

                                        if (valueBytes != null)
                                        {
                                            map.Add("value", getHexString(valueBytes));
                                        }
                                        else
                                        {
                                            map.Add("value", "");
                                        }
					    			
					    			    fillMaps.Add(map);								

	    				    		    iTLV += lengthValue;
								    }
							    }

							    bTag = true;
    					    }    					
    				    }
				    }
    		    }
    	    }

    	    return fillMaps;
        }

        public static String getTagValue(List<Dictionary<String, String>> fillMaps, String tagString)
        {
            String valueString = "";
            String tagField;

            foreach (Dictionary<String, String> map in fillMaps)
            {
                if (map.TryGetValue("tag", out tagField))
                {
                    if (String.Compare(tagString, tagField, true) == 0)
                    {
                        if (map.TryGetValue("value", out valueString))
                        {
                            break;
                        }
                    }
                }
            }

            return valueString;
        }

        public static string getHexString(byte[] data)
        {
            StringBuilder hexString = new StringBuilder(data.Length * 2);

            try 
            {
                foreach (byte byteValue in data)
                {
                    hexString.AppendFormat("{0:X2}", byteValue);
                }
            }
            catch (Exception)
            {
            }

            return hexString.ToString();
        }

        public static byte[] getByteArrayFromHexString(string str)
        {
            if (str == null)
                return null;

            // Determine how many bytes are needed.
            int len = str.Length >> 1;

            if (len < 1)
            {
                return null;
            }

            byte[] bytes = new byte[str.Length >> 1];

            try 
            {
                for (int i = 0; i < str.Length; i += 2)
                {
                    int highDigit = hexDigits.IndexOf(Char.ToUpperInvariant(str[i]));
                    int lowDigit = hexDigits.IndexOf(Char.ToUpperInvariant(str[i + 1]));
                    if (highDigit == -1 || lowDigit == -1)
                    {
//                        throw new ArgumentException("The string contains an invalid digit.", "s");
                    }
                    bytes[i >> 1] = (byte)((highDigit << 4) | lowDigit);
                }
            }
            catch (Exception)
            {
            }

            return bytes;
        }

        public static String getTwoByteLengthString(int length)
        {
            byte[] lengthBytes = new byte[2];

            lengthBytes[0] = (byte)((length >> 8) & 0xFF);
            lengthBytes[1] = (byte)(length & 0xFF);

            return getHexString(lengthBytes);
        }

       
           
    }
}
