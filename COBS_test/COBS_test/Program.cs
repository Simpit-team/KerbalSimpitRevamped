using System;
using System.Linq;

namespace COBS_test
{
    class Program
    {

        /// <summary>
        /// Decode a COBS-encoded array of bytes (assuming a size < 256 bytes).
        /// 
        /// Will parse the input the first 0 is seen. If this is not the last byte, it will discard the remaining content and return false. 
        /// </summary>
        /// <param name="input">Buffer for the input. </param>
        /// <param name="output">Buffer for the output. Will be allocated in the function. </param>
        /// <returns>True if the decoding is successful </returns>
        static bool decodeCOBS(in byte[] input, out byte[] output)
        {
            // Output will be the same size as the input, minus 1 byte of overhead and one byte of the terminating null byte.
            output = new byte[input.Length - 2];
            if (input.Length >= 255)
                return false;

            int nextZero = input[0];
            for (int i = 1; i< input.Length; i++)
            {
                if(input[i] == 0)
                {
                    return (i == input.Length - 1) && (nextZero == 1);
                }

                nextZero--;
                if(nextZero == 0)
                {
                    output[i - 1] = 0;
                    nextZero = input[i];
                } else {
                    output[i - 1] = input[i];
                }
            }

            return false;
        }

        /// <summary>
        /// Encode a COBS-encoded array of bytes (assuming a size < 256 bytes).
        /// 
        /// </summary>
        /// <param name="input">Buffer for the input. </param>
        /// <param name="output">Buffer for the output. Will be allocated in the function and will be terminated with a null byte. </param>
        /// <returns>True if the encoding is successful </returns>
        static bool encodeCOBS(in byte[] input, out byte[] output)
        {
            // Output will be the same size as the input, plus 1 byte of overhead and one byte of the terminating null byte.
            output = new byte[input.Length + 2];
            if (input.Length >= 255)
                return false;
            if (output.Length < input.Length + 2)
                return false;

            uint lastZero = 0;
            byte distanceLastZero = 1;
            for (uint i = 0; i < input.Length; i++){
                //coding byte at position i of the inputBuffer, should go at position i+1 of the output buffer.
                if(input[i] == 0)
                {
                    output[lastZero] = distanceLastZero;
                    lastZero = i + 1;
                    distanceLastZero = 1;
                } else
                {
                    output[i + 1] = input[i];
                    distanceLastZero++;
                }
            }

            output[lastZero] = distanceLastZero;
            output[input.Length + 1] = 0;
            return true;
        }

        /// <summary>
        /// Encode a packet (defined as a type and a payload) with a checksum and output a COBS-encoded message ready to be sent
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="payload"></param>
        /// <param name="output">Buffer for the output. Will be allocated in the function and will be terminated with a null byte. </param>
        static void encodePacket(in byte packetType, in byte[] payload, out byte[] output)
        {
            byte[] buffer = new byte[payload.Length + 2];
            buffer[0] = packetType;
            byte checksum = packetType;
            for(int i = 0; i < payload.Length; i++)
            {
                checksum ^= payload[i];
                buffer[i + 1] = payload[i];
            }
            buffer[payload.Length + 1] = checksum;
            encodeCOBS(buffer, out output);
        }

        /// <summary>
        /// Decode a packet (checking its integrity with COBS and the checksum included in the message) and output the packet type and its payload.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="packetType"></param>
        /// <param name="payload">Will be allocated in the function to the right size</param>
        /// <returns>false if the packet is not validated (and should be discarded). </returns>
        static bool decodePacket(in byte[] input, out byte packetType, out byte[] payload)
        {
            if (input.Length <= 4) {
                // Not enough data to have a packet type, a payload, a checksum and the additionnal byte of COBS encoding
                packetType = 0;
                payload = null;
                return false;
            }
            payload = new byte[input.Length - 4];

            byte[] buffer;
            bool sucess = decodeCOBS(input, out buffer);

            if (!sucess) {
                // COBS was ill-formed, discarding the message
                packetType = 0;
                payload = null;
                return false;
            }

            byte checksum = 0;
            for(int i = 0; i < buffer.Length - 1; i++)
            {
                checksum ^= buffer[i];
            }

            // If checksum do not match, return false
            if (checksum != buffer[buffer.Length - 1]) {
                Console.WriteLine("Computed checksum " + checksum + " != from " + buffer[buffer.Length - 1]);
                packetType = 0;
                return false;
            }

            packetType = buffer[0];
            Array.Copy(buffer, 1, payload, 0, buffer.Length - 2);
            return true;
        }

        static void TestCOBSDecoding()
        {
            byte[] inputBuffer = new byte[] { 1, 2, 11, 1, 0 };
            byte[] expectedOutputBuffer = new byte[] { 0, 11, 0 };
            byte[] outputBuffer;

            bool success = decodeCOBS(inputBuffer, out outputBuffer);

            Console.WriteLine("Test 1. success ? " + success + ". Expected output ? " + outputBuffer.Take(expectedOutputBuffer.Length).SequenceEqual(expectedOutputBuffer));

            inputBuffer = new byte[] { 05, 11, 22, 33, 44, 00 };
            expectedOutputBuffer = new byte[] { 11, 22, 33, 44 };
            for (int i = 0; i < outputBuffer.Length; i++) { outputBuffer[i] = 0x20; }
            success = decodeCOBS(inputBuffer, out outputBuffer);
            Console.WriteLine("Test 2. success ? " + success + ". Expected output ? " + outputBuffer.Take(expectedOutputBuffer.Length).SequenceEqual(expectedOutputBuffer));

            inputBuffer = new byte[] { 02, 11, 01, 01, 01, 00 };
            expectedOutputBuffer = new byte[] { 11, 00, 00, 00 };
            for (int i = 0; i < outputBuffer.Length; i++) { outputBuffer[i] = 0x20; }
            success = decodeCOBS(inputBuffer, out outputBuffer);
            Console.WriteLine("Test 3. success ? " + success + ". Expected output ? " + outputBuffer.Take(expectedOutputBuffer.Length).SequenceEqual(expectedOutputBuffer));

            inputBuffer = new byte[] { 02, 11, 01, 00, 01, 01, 00 };
            for (int i = 0; i < outputBuffer.Length; i++) { outputBuffer[i] = 0x20; }
            success = decodeCOBS(inputBuffer, out outputBuffer);
            Console.WriteLine("Test 4. success ? " + !success);

            inputBuffer = new byte[] { 02, 11, 01, 01, 02, 00 };
            for (int i = 0; i < outputBuffer.Length; i++) { outputBuffer[i] = 0x20; }
            success = decodeCOBS(inputBuffer, out outputBuffer);
            Console.WriteLine("Test 5. success ? " + !success);
        }

        static void TestCOBSEncoding()
        {
            byte[] inputBuffer = new byte[] { 0 };
            byte[] expectedOutputBuffer = new byte[] { 1, 1, 0 };
            byte[] outputBuffer;

            bool success = encodeCOBS(inputBuffer, out outputBuffer);

            Console.WriteLine("Test 1. success ? " + success + ". Expected output ? " + outputBuffer.Take(expectedOutputBuffer.Length).SequenceEqual(expectedOutputBuffer));

            inputBuffer = new byte[] { 11, 22, 00, 33 };
            expectedOutputBuffer = new byte[] { 03, 11, 22, 02, 33, 00 };
            for (int i = 0; i < outputBuffer.Length; i++) { outputBuffer[i] = 0x20; }
            success = encodeCOBS(inputBuffer, out outputBuffer);
            Console.WriteLine("Test 2. success ? " + success + ". Expected output ? " + outputBuffer.Take(expectedOutputBuffer.Length).SequenceEqual(expectedOutputBuffer));

            inputBuffer = new byte[] { 11, 00, 00, 00 };
            expectedOutputBuffer = new byte[] { 02, 11, 01, 01, 01, 00 };
            for (int i = 0; i < outputBuffer.Length; i++) { outputBuffer[i] = 0x20; }
            success = encodeCOBS(inputBuffer, out outputBuffer);
            Console.WriteLine("Test 3. success ? " + success + ". Expected output ? " + outputBuffer.Take(expectedOutputBuffer.Length).SequenceEqual(expectedOutputBuffer));
        }

        static void TestEncodeDecode()
        {
            byte packetType = 27;
            byte outputPacketType = 0;
            byte[] inputBuffer = new byte[] { 0, 2, 3 };
            byte[] transmitBuffer, outputBuffer;

            encodePacket(packetType, inputBuffer, out transmitBuffer);
            bool success = decodePacket(transmitBuffer, out outputPacketType, out outputBuffer);
            Console.WriteLine("Test 1. success ? " + success + ". Expected output ? " + inputBuffer.SequenceEqual(outputBuffer));

            encodePacket(packetType, inputBuffer, out transmitBuffer);
            // Test an ill-formed COBS encoding
            transmitBuffer[2] += 1;
            success = decodePacket(transmitBuffer, out outputPacketType, out outputBuffer);
            Console.WriteLine("Test 2. success ? " + !success);


            encodePacket(packetType, inputBuffer, out transmitBuffer);
            // Test the checksum computation
            transmitBuffer[4] += 1;
            success = decodePacket(transmitBuffer, out outputPacketType, out outputBuffer);
            Console.WriteLine("Test 3. success ? " + !success);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Testing COBS decoding");
            TestCOBSDecoding();

            Console.WriteLine("Testing COBS encoding");
            TestCOBSEncoding();

            Console.WriteLine("Testing encoding/decoding");
            TestEncodeDecode();

            Console.WriteLine("Tests finished");
        }
    }
}
