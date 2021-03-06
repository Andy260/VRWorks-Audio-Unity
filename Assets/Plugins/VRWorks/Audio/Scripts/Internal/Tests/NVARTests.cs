﻿using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NVIDIA.VRWorksAudio.Internal.Tests
{
    [TestFixture]
    public sealed class ErrorHandlingTests
    {
        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetStatusDescription() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetStatusDescription()
        {
            // Status enumeration to get the description string from NVAR
            NVAR.Status testingStatus = NVAR.Status.Error;

            // Get status enumeration string description from NVAR
            string description;
            NVAR.Status status = NVAR.GetStatusDescription(out description, testingStatus);

            // Ensure call to NVAR succeeded
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetStatusDescription() failed");

            // Ensure returned value is as expected
            Assert.AreEqual("The API returned an unspecified error.", description);
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetStatusString() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetStatusString()
        {
            // Status enumeration to get the string name representation from NVAR
            NVAR.Status testingStatus = NVAR.Status.Error;

            // Get status enumeration string name from NVAR
            string name;
            NVAR.Status status = NVAR.GetStatusString(out name, testingStatus);

            // Ensure call to NVAR succeeded
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetStatusString() failed");

            // Ensure returned value is as expected
            Assert.AreEqual("NVAR_STATUS_ERROR", name);
        }
    }

    [TestFixture]
    [SingleThreaded]
    public sealed class GeneralTests
    {
        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetVersion() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetVersion()
        {
            // Get NVAR version
            int version;
            NVAR.Status status = NVAR.GetVersion(out version);

            // Ensure NVAR call succeded
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetVersion() failed");
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetOutputFormatChannels() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetOutputFormatChannels()
        {
            NVAR.OutputFormat outputFormat = NVAR.OutputFormat.OutputFormatStereoHeadphones;

            // Get Output format channels count
            int channels;
            NVAR.Status status = NVAR.GetOutputFormatChannels(outputFormat, out channels);

            // Ensure call to NVAR suceeded
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetOutputFormatChannels() failed");

            // Ensure returned values are as expected
            Assert.AreEqual(2, channels);
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarIntialize() and nvarFinalize() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void InitializeAndFinalize()
        {
            // Test initialising NVAR
            NVAR.Status status = NVAR.Initialize(0);
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarInitialize() failed");

            // Test finalising NVAR
            status = NVAR.Finalize();
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarFinalize() failed");
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetInitializeFlags() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetInitializeFlags()
        {
            int initialiseFlags = 0;
            TestHelper.InitialiseNVAR(initialiseFlags);
            {
                // Get Intiailise flags from NVAR
                int returnValue;
                NVAR.Status status = NVAR.GetInitializeFlags(out returnValue);

                // Ensure call to NVAR succeded
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetInitializeFlags() failed");

                // Ensure returned value is expected value
                Assert.AreEqual(initialiseFlags, returnValue, "Initialisation flags not expected value");
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetDeviceCount() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetDeviceCount()
        {
            TestHelper.InitialiseNVAR(0);
            {
                int deviceCount = -1;
                NVAR.Status status = NVAR.GetDeviceCount(out deviceCount);

                // Ensure NVAR call succeded
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDeviceCount() failed");
                // Ensure expected value within expected range
                // (Will generate different values on different machines, so no specific value is guranteed)
                Assert.GreaterOrEqual(deviceCount, 0, "No devices returned by NVAR");
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetDevices() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetDevices()
        {
            TestHelper.InitialiseNVAR(0);
            {
                // Get CUDA ordinals from NVAR
                int[] devices;
                NVAR.Status status = NVAR.GetDevices(out devices);

                // Ensure NVAR call succeded
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDevices() failed");

                // Attempt to get CUDA ordinal count to ensure
                // returned data is consistent
                int deviceCount;
                status = NVAR.GetDeviceCount(out deviceCount);
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDeviceCount() failed");

                // Ensure returned value is expected value
                Assert.IsNotNull(devices, "Returned NVAR devices NULL");
                Assert.AreEqual(deviceCount, devices.Length, "Returned devices array size not consistent with device count");
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetDeviceName() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetDeviceName()
        {
            TestHelper.InitialiseNVAR(0);
            {
                // Get list of device IDs
                int[] devices;
                NVAR.Status status = NVAR.GetDevices(out devices);
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDevices() failed");

                // Ensure we have devices to excecute this test
                if (devices.Length < 1)
                {
                    Assert.Fail("No CUDA devices returned from NVAR");
                }

                // Get device 0's name
                string deviceName;
                status = NVAR.GetDeviceName(devices[0], out deviceName);
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDeviceName() failed");

                // Ensure returned string is valid
                Assert.IsFalse(string.IsNullOrEmpty(deviceName), "Returned device name is empty/NULL");
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetPreferedDevice() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetPreferedDevice()
        {
            TestHelper.InitialiseNVAR(0);
            {
                int preferedDevice = -1;
                NVAR.Status status = NVAR.GetPreferedDevice(out preferedDevice);

                // Ensure call to NVAR suceeded
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetPreferedDevice() failed");

                // Ensure returned value is as expected
                Assert.AreNotEqual(-1, preferedDevice, "Invalid value returned as prefered CUDA device");
            }
            TestHelper.FinaliseNVAR();
        }
    }

    [TestFixture]
    [SingleThreaded]
    public sealed class ProcessingContextTests
    {
        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarCreate() and nvarDestroy() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void CreateAndDestroy()
        {
            TestHelper.InitialiseNVAR(0);
            {
                // Get number of CUDA devices
                int deviceCount;
                NVAR.Status status = NVAR.GetDeviceCount(out deviceCount);

                // Ensure the request for device count in NVAR succeeded
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDeviceCount() failed");
                // Ensure we have at least one CUDA device on this machine to perform this unit test
                if (deviceCount <= 0)
                {
                    Assert.Fail("No CUDA devices available on this machine");
                }

                // Define NVAR context properties
                string contextName          = "Test Context";
                NVAR.EffectPreset preset    = NVAR.EffectPresetDefault;
                int deviceNum               = 0;

                // Create NVAR context
                NVAR.Context context;
                status = NVAR.Create(out context, contextName, preset, deviceNum);

                // Ensure NVAR context creation succeded
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreate() failed");

                // Destroy NVAR context
                status = NVAR.Destroy(context);

                // Ensure NVAR context was successfully destroyed
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroy() failed");
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarCommitGeometry() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void CommitGeometry()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Ensure calls to nvarCommitGeomtry don't fail
                    NVAR.Status status = NVAR.CommitGeometry(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCommitGeometry() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarEventRecord() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void EventRecord()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    using (AutoResetEvent traceCompleteEvent = new AutoResetEvent(false))
                    {
                        // Increment reference counter in thread sync event
                        bool success = false;
                        traceCompleteEvent.SafeWaitHandle.DangerousAddRef(ref success);

                        // Ensure we were able to get a native handle to the thread synchronisation object
                        if (!success)
                        {
                            Assert.Fail("Failed to retrieve native handle to thread synchronisation object");
                        }

                        // Add synchronise event with NVAR
                        NVAR.Status status = NVAR.EventRecord(context, traceCompleteEvent.SafeWaitHandle.DangerousGetHandle());
                        Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarEventRecord() failed");

                        // Synchronise with NVAR
                        bool eventSignalled = traceCompleteEvent.WaitOne(new TimeSpan(0, 0, 5));
                        Assert.IsTrue(eventSignalled, "Unable to synchronise calling thread with NVAR");

                        // Release handle to native sync event
                        traceCompleteEvent.SafeWaitHandle.DangerousRelease();
                    }
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarCommitGeometry() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void ExportOBJs()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    const string filePath = "ExportOBJ_Test";

                    // Destroy files created from a previous test, if any
                    if (File.Exists(filePath + ".obj"))
                    {
                        File.Delete(filePath + ".obj");
                    }
                    if (File.Exists(filePath + ".mtl"))
                    {
                        File.Delete(filePath + ".mtl");
                    }

                    // Create NVAR OBJ export file
                    NVAR.Status status = NVAR.ExportOBJs(context, filePath);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarExportOBJs() failed");

                    // Wait for NVAR to idle
                    status = NVAR.Synchronize(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSyncronize() failed");

                    // Ensure we have a file created
                    FileAssert.Exists(filePath + ".obj");

                    // Destroy generated files, if created
                    if (File.Exists(filePath + ".obj"))
                    {
                        File.Delete(filePath + ".obj");
                    }
                    if (File.Exists(filePath + ".mtl"))
                    {
                        File.Delete(filePath + ".mtl");
                    }
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetDecayFactor() and nvarSetDecayFactor() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetDecayFactor()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Decay factor value to set to
                    float decayFactor = 1f;

                    // Set decay factor in NVAR context
                    NVAR.Status status = NVAR.SetDecayFactor(context, decayFactor);
                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetDecayFactor() failed");

                    // Retrieve current NVAR decay factor
                    float nvarDecayFactor;
                    status = NVAR.GetDecayFactor(context, out nvarDecayFactor);
                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDecayFactor() failed");

                    // Ensure NVAR decay factor is consistent with set value
                    Assert.AreEqual(decayFactor, nvarDecayFactor, "NVAR set decay factor isn't expected value");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetDeviceNum() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetDeviceNum()
        {
            TestHelper.InitialiseNVAR(0);
            {
                // Device to create the NVAR context with
                int deviceID = 0;

                // Get number of CUDA devices
                int deviceCount;
                NVAR.Status status = NVAR.GetDeviceCount(out deviceCount);

                // Ensure the request for device count in NVAR succeeded
                Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDeviceCount() failed");
                // Ensure we have at least one CUDA device on this machine to perform this unit test
                if (deviceCount <= 0)
                {
                    Assert.Fail("No CUDA devices available on this machine");
                }

                NVAR.Context context = TestHelper.CreateNVARContext(deviceID);
                {
                    // Get device used by the NVAR context
                    int nvarDevice;
                    status = NVAR.GetDeviceNum(context, out nvarDevice);

                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDeviceNum() failed");

                    // Ensure given device from the NVAR is as expected
                    Assert.AreEqual(deviceID, nvarDevice, "NVAR not using expected device");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetListenerLocation() and nvarSetListenerLocation() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetListenerLocation()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Set NVAR listener location
                    Vector3 newListenerLocation = new Vector3(1f, 2f, 3f);
                    NVAR.Status status          = NVAR.SetListenerLocation(context, newListenerLocation);

                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetListenerLocation() failed");

                    // Get NVAR listener location
                    Vector3 nvarListenerLocation;
                    status = NVAR.GetListenerLocation(context, out nvarListenerLocation);

                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetListenerLocation() failed");

                    // Ensure NVAR location and set location are the same
                    Assert.AreEqual(newListenerLocation, nvarListenerLocation, "NVAR listener location not expected value");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetListenerOrientation() and nvarSetListenerOrientation() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetListenerOrientation()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Set NVAR listener orientation
                    Vector3 orientationForward  = Vector3.forward;
                    Vector3 orientationUp       = Vector3.up;
                    NVAR.Status status          = NVAR.SetListenerOrientation(context, orientationForward, orientationUp);

                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetListenerOrientation() failed");

                    // Get NVAR listener orientation
                    Vector3 nvarOrientationForward;
                    Vector3 nvarOrientationUp;
                    status = NVAR.GetListenerOrientation(context, out nvarOrientationForward, out nvarOrientationUp);

                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetListenerOrientation() failed");

                    // Ensure NVAR orientation and set orientation are the same
                    Assert.AreEqual(orientationForward, nvarOrientationForward, "NVAR listener orientation forward vector not expected value");
                    Assert.AreEqual(orientationUp, nvarOrientationUp, "NVAR listener orientation up vector not expected value");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetOutputFormat() and nvarSetOutputFormat() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetOutputFormat()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Set NVAR output format
                    NVAR.OutputFormat newOutputFormat = NVAR.OutputFormat.OutputFormatStereoHeadphones;
                    NVAR.Status status = NVAR.SetOutputFormat(context, newOutputFormat);

                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetOutputFormat() failed");

                    // Get current NVAR output format
                    NVAR.OutputFormat nvarOutputFormat;
                    status = NVAR.GetOutputFormat(context, out nvarOutputFormat);

                    // Ensure call to NVAR succeeded
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetOutputFormat() failed");

                    // Ensure NVAR output format is as expected
                    // (this doesn't actually test anything since the enumeration only has one value
                    // but I've wrote this for when the NVAR API supports different output formats)
                    Assert.AreEqual(newOutputFormat, nvarOutputFormat, "NVAR output format not expected value");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetReverbLength() and nvarSetReverbLength() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetReverbLength()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    float newReverbLength = 2f;

                    // Set NVAR reverb length
                    NVAR.Status status = NVAR.SetReverbLength(context, newReverbLength);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetReverbLength() failed");

                    // Get current NVAR reverb length value
                    float nvarReverbLength;
                    status = NVAR.GetReverbLength(context, out nvarReverbLength);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetReverbLength() failed");

                    // Ensure set NVAR reverb length value is as expected
                    Assert.AreEqual(newReverbLength, nvarReverbLength, "NVAR reverb length not expected value");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetSampleRate() and nvarSetSampleRate() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetSampleRate()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    int sampleRate = 48000;

                    // Set NVAR sample rate
                    NVAR.Status status = NVAR.SetSampleRate(context, sampleRate);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetSampleRate() failed");

                    // Get current NVAR sample rate
                    int nvarSampleRate;
                    status = NVAR.GetSampleRate(context, out nvarSampleRate);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetSampleRate() failed");

                    // Ensure set NVAR sample rate is as expected
                    Assert.AreEqual(sampleRate, nvarSampleRate, "NVAR sample rate not expected value");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetUnitLength() and nvarSetUnitLength() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetUnitLength()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    float unitLengthRatio = 0.01f;

                    // Set NVAR unit length ratio
                    NVAR.Status status = NVAR.SetUnitLength(context, unitLengthRatio);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetUnitLength() failed");

                    // Get NVAR unit length ratio
                    float nvarUnitLengthRatio;
                    status = NVAR.GetUnitLength(context, out nvarUnitLengthRatio);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetUnitLength() failed");

                    // Ensure NVAR unit length ratio is expected value
                    Assert.AreEqual(unitLengthRatio, nvarUnitLengthRatio, "NVAR unit length ratio not expected value");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarSynchronize() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void Synchronize()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    NVAR.Status status = NVAR.Synchronize(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSynchronize() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarTraceAudio() in the managed API with a_traceDoneEvent NULL")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void TraceAudio()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Ensure calls to nvarTraceAudio() don't fail
                    NVAR.Status status = NVAR.TraceAudio(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarTraceAudio() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarTraceAudio() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void TraceAudioFull()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    using (AutoResetEvent traceCompleteEvent = new AutoResetEvent(false))
                    {
                        // Increment reference counter in thread sync event
                        bool success = false;
                        traceCompleteEvent.SafeWaitHandle.DangerousAddRef(ref success);

                        // Ensure we were able to get a native handle to the thread synchronisation object
                        if (!success)
                        {
                            Assert.Fail("Failed to retrieve native handle to thread synchronisation object");
                        }

                        // Trace audio in NVAR
                        NVAR.Status status = NVAR.TraceAudio(context, traceCompleteEvent.SafeWaitHandle.DangerousGetHandle());
                        Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarTraceAudio() failed");

                        // Synchronise with NVAR
                        bool eventSignalled = traceCompleteEvent.WaitOne(new TimeSpan(0, 0, 5));
                        Assert.IsTrue(eventSignalled, "Unable to synchronise calling thread with NVAR");

                        // Release handle to native sync event
                        traceCompleteEvent.SafeWaitHandle.DangerousRelease();
                    }
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }
    }

    [TestFixture]
    [SingleThreaded]
    public sealed class AcousticMaterialTests
    {
        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarCreateMaterial() and nvarDestroyMaterial() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void CreateAndDestroyMaterial()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create NVAR material
                    NVAR.Material material;
                    NVAR.Status status = NVAR.CreateMaterial(context, out material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMaterial() failed");

                    // Destroy NVAR material
                    status = NVAR.DestroyMaterial(material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMaterial() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarCreatePredefinedMaterial() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void CreatePredefinedMaterial()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create NVAR material
                    NVAR.Material material;
                    NVAR.Status status = NVAR.CreatePredefinedMaterial(context, out material, NVAR.PredefinedMaterial.Concrete);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreatePredefinedMaterial() failed");

                    // Destroy NVAR material
                    status = NVAR.DestroyMaterial(material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMaterial() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetMaterialReflection() and nvarSetMaterialReflection() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetMaterialReflection()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create acoustic material
                    NVAR.Material material;
                    NVAR.Status status = NVAR.CreateMaterial(context, out material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMaterial() failed");

                    // Set acoustic material reflection value
                    float reflection = 1f;
                    status = NVAR.SetMaterialReflection(material, reflection);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetMaterialReflection() failed");

                    // Get acoustic material reflection value
                    float nvarReflection;
                    status = NVAR.GetMaterialReflection(material, out nvarReflection);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetMaterialReflection() failed");

                    // Ensure NVAR material reflection value is as expected
                    Assert.AreEqual(reflection, nvarReflection, "NVAR material reflection value not expected value");

                    // Destroy acoustic material
                    status = NVAR.DestroyMaterial(material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMaterial() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetMaterialTransmission() and nvarSetMaterialTransmission() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetMaterialTransmission()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create acoustic material
                    NVAR.Material material;
                    NVAR.Status status = NVAR.CreateMaterial(context, out material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMaterial() failed");

                    // Set acoustic material transmission value
                    float transmission = 1f;
                    status = NVAR.SetMaterialTransmission(material, transmission);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetMaterialTransmission() failed");

                    // Get acoustic material transmission value
                    float nvarTransmission;
                    status = NVAR.GetMaterialTransmission(material, out nvarTransmission);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetMaterialTransmission() failed");

                    // Ensure NVAR material transmission value is as expected
                    Assert.AreEqual(transmission, nvarTransmission, "NVAR material transmission value not expected value");

                    // Destroy acoustic material
                    status = NVAR.DestroyMaterial(material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMaterial() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }
    }

    [TestFixture]
    [SingleThreaded]
    public sealed class AcousticMeshTests
    {
        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarCreateMaterial() and nvarDestroyMaterial() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void CreateAndDestroyMesh()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create acoustic material to attach to acoustic mesh
                    NVAR.Material material;
                    NVAR.Status status = NVAR.CreateMaterial(context, out material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMaterial() failed");

                    // Create Unity mesh to create NVAR mesh from
                    GameObject cubeObject   = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Mesh cubeMesh           = cubeObject.GetComponent<MeshFilter>().sharedMesh;

                    // Create acoustic NVAR mesh
                    NVAR.Mesh mesh;
                    status = NVAR.CreateMesh(context, out mesh, cubeObject.transform.worldToLocalMatrix, cubeMesh.vertices, cubeMesh.triangles, material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMesh() failed");

                    // Destroy acoustic mesh
                    status = NVAR.DestroyMesh(mesh);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMesh() failed");

                    // Destroy acoustic material
                    status = NVAR.DestroyMaterial(material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMesh() failed");
                    // Destroy primitive cube object
                    Object.DestroyImmediate(cubeObject);
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetMeshMaterial() and nvarSetMeshMaterial() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetMeshMaterial()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create initially attached acoustic material
                    NVAR.Material attachedMaterial;
                    NVAR.Status status = NVAR.CreateMaterial(context, out attachedMaterial);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMaterial() failed");

                    // Create Unity mesh to create NVAR mesh from
                    GameObject cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Mesh cubeMesh = cubeObject.GetComponent<MeshFilter>().sharedMesh;

                    // Create acoustic NVAR mesh
                    NVAR.Mesh mesh;
                    status = NVAR.CreateMesh(context, out mesh, cubeObject.transform.worldToLocalMatrix, cubeMesh.vertices, cubeMesh.triangles, attachedMaterial);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMesh() failed");

                    // Create acoustic material to swap on created acoustic mesh
                    NVAR.Material swapMaterial;
                    status = NVAR.CreateMaterial(context, out swapMaterial);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMaterial() failed");

                    // Set acoustic material on created acoustic mesh
                    status = NVAR.SetMeshMaterial(mesh, swapMaterial);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetMeshMaterial() failed");

                    // Get current material on acoustic mesh
                    NVAR.Material nvarMaterial;
                    status = NVAR.GetMeshMaterial(mesh, out nvarMaterial);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetMeshMaterial() failed");

                    // Ensure acoustic material is swapped acoustic material on created acoustic mesh
                    Assert.AreEqual(swapMaterial, nvarMaterial, "NVAR material on acoustic mesh not expected material");

                    // Destroy acoustic mesh
                    status = NVAR.DestroyMesh(mesh);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMesh() failed");
                    // Destroy initially attached acoustic material
                    status = NVAR.DestroyMaterial(attachedMaterial);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMaterial() failed");
                    // Destroy acoustic material swapping onto acoustic mesh
                    status = NVAR.DestroyMaterial(swapMaterial);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMaterial() failed");
                    // Destroy primitive cube object
                    Object.DestroyImmediate(cubeObject);
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetMeshTransform() and nvarSetMeshTransform() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetMeshTransform()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create acoustic material to attach to acoustic mesh
                    NVAR.Material material;
                    NVAR.Status status = NVAR.CreateMaterial(context, out material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMaterial() failed");

                    // Create Unity mesh to create NVAR mesh from
                    GameObject cubeObject   = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Mesh cubeMesh           = cubeObject.GetComponent<MeshFilter>().sharedMesh;

                    // Set initial position of cube object to base acoustic mesh's transform from
                    cubeObject.transform.position   = new Vector3(100, 200, 300);
                    Matrix4x4 initialCubeMatrix     = cubeObject.transform.worldToLocalMatrix;

                    // Create acoustic NVAR mesh
                    NVAR.Mesh mesh;
                    status = NVAR.CreateMesh(context, out mesh, initialCubeMatrix, cubeMesh.vertices, cubeMesh.triangles, material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateMesh() failed");

                    // Move cube object, so we can move the acoustic mesh
                    cubeObject.transform.position   = new Vector3(-100, 50, 200);
                    Matrix4x4 newCubeMatrix         = cubeObject.transform.worldToLocalMatrix;

                    // Set new mesh transform
                    status = NVAR.SetMeshTransform(mesh, newCubeMatrix);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetMeshTransform() failed");

                    // Get current transform in NVAR of acoustic mesh
                    Matrix4x4 nvarTransform;
                    status = NVAR.GetMeshTransform(mesh, out nvarTransform);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetMeshTransform() failed");

                    // Ensure new mesh transform in NVAR is as expected
                    Assert.AreEqual(newCubeMatrix, nvarTransform, "NVAR acoustic mesh transform not expected value");

                    // Destroy acoustic mesh
                    status = NVAR.DestroyMesh(mesh);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMesh() failed");

                    // Destroy acoustic material
                    status = NVAR.DestroyMaterial(material);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroyMesh() failed");
                    // Destroy primitive cube object
                    Object.DestroyImmediate(cubeObject);
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }
    }

    [TestFixture]
    [SingleThreaded]
    public sealed class SoundSourceTests
    {
        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarCreateSource() and nvarDestroySource() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void CreateAndDestroySource()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create NVAR sound source
                    NVAR.Source soundSource;
                    NVAR.Status status = NVAR.CreateSource(context, NVAR.EffectPreset.High, out soundSource);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateSource() failed");

                    // Destroy NVAR sound source
                    status = NVAR.DestroySource(soundSource);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroySource() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarApplySourceFilters() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void ApplySourceFilters()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    int sampleCount = 44100;

                    // Create sound source
                    NVAR.Source source;
                    NVAR.Status status = NVAR.CreateSource(context, NVAR.EffectPreset.High, out source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateSource() failed");

                    // Begin audio trace and synchronise this thread with NVAR
                    status = NVAR.TraceAudio(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarTraceAudio() failed");
                    status = NVAR.Synchronize(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSyncronise() failed");

                    // Create sample audio data
                    // (typical data rate is 44100HZ called 20ms per tick within Unity's audio thread
                    // (and VRWorks Audio only supports stereo)
                    float[] audioData = new float[sampleCount];

                    // We will generate random audio data with a predictable seed for
                    // consistent test execution, so we can be sure our NVAR binding 
                    // is correctly using our data
                    for (int i = 0; i < audioData.Length; ++i)
                    {
                        // Audio data from Unity ranges from -1 - 1 
                        // (see MonoBehaviour.OnAudioFilterRead(float[], int))
                        audioData[i] = Random.Range(-1f, 1f);
                    }

                    // Apply filters from NVAR
                    float[][] filterData;
                    status = NVAR.ApplySourceFilters(source, out filterData, audioData, sampleCount);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarApplySourceFilters() failed");

                    // Verify filter data returned from NVAR
                    Assert.IsNotNull(filterData);
                    foreach (float[] dataArray in filterData)
                    {
                        Assert.IsNotNull(dataArray);
                    }
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetSourceFilterArraySize() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetSourceFilterArraySize()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Get source filter array size from NVAR
                    int nvarSourceFilterArraySize = -1;
                    NVAR.Status status = NVAR.GetSourceFilterArraySize(context, out nvarSourceFilterArraySize);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetSourceFilterArraySize() failed");

                    // Ensure a value was returned by NVAR
                    Assert.AreNotEqual(-1, nvarSourceFilterArraySize, "No value returned from NVAR for filter array size");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetSourceFilters() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetSourceFilters()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create NVAR sound source
                    NVAR.Source source;
                    NVAR.Status status = NVAR.CreateSource(context, NVAR.EffectPreset.High, out source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateSource() failed");

                    // Trace audio
                    status = NVAR.TraceAudio(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarTraceAudio() failed");

                    // Wait for NVAR to idle
                    status = NVAR.Synchronize(context);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSynchronize() failed");

                    // Get NVAR sound source filter array
                    float[] filterArray;
                    status = NVAR.GetSourceFilters(context, source, out filterArray);

                    // Ensure the filter array returned from NVAR isn't NULL
                    Assert.NotNull(filterArray, "Filter array object is NULL");

                    // Get source filter array size from NVAR
                    int nvarSourceFilterArraySize;
                    status = NVAR.GetSourceFilterArraySize(context, out nvarSourceFilterArraySize);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetSourceFilterArraySize() failed");

                    // Ensure returned NVAR sound source filter array is expected length
                    Assert.AreEqual(nvarSourceFilterArraySize, filterArray.Length, "Filter array not expected length");

                    // Destroy NVAR sound source
                    status = NVAR.DestroySource(source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroySource() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetSourceDirectGain() and nvarSetSourceDirectGain() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetSourceDirectGain()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create NVAR sound source
                    NVAR.Source source;
                    NVAR.Status status = NVAR.CreateSource(context, NVAR.EffectPreset.High, out source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateSource() failed");

                    // Set source direct gain value
                    float directGain = 2f;
                    status = NVAR.SetSourceDirectPathGain(source, directGain);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetSourceDirectPathGain() failed");

                    // Get source direct gain in NVAR
                    float nvarDirectGain;
                    status = NVAR.GetSourceDirectPathGain(source, out nvarDirectGain);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetSourceDirectPathGain() failed");

                    // Ensure NVAR source direct gain is as expected
                    Assert.AreEqual(directGain, nvarDirectGain, "NVAR source direct gain not expected value");

                    // Destroy NVAR sound source
                    status = NVAR.DestroySource(source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroySource() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetSourceIndirectGain() and nvarSetSourceIndirectGain() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetSourceIndirectGain()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create NVAR sound source
                    NVAR.Source source;
                    NVAR.Status status = NVAR.CreateSource(context, NVAR.EffectPreset.High, out source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateSource() failed");

                    // Set source indirect gain value
                    float indirectGain = 5f;
                    status = NVAR.SetSourceIndirectPathGain(source, indirectGain);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetSourceIndirectPathGain() failed");

                    // Get source indirect gain in NVAR
                    float nvarIndirectGain;
                    status = NVAR.GetSourceIndirectPathGain(source, out nvarIndirectGain);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetSourceIndirectPathGain() failed");

                    // Ensure NVAR source indirect gain is as expected
                    Assert.AreEqual(indirectGain, nvarIndirectGain, "NVAR source indirect gain not expected value");

                    // Destroy NVAR sound source
                    status = NVAR.DestroySource(source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroySource() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }

        [Test]
        [Category("Managed Binding API Test")]
        [Description("Tests nvarGetSourceLocation() and nvarSetSourceLocation() in the managed API")]
        [RequirePlatformSupport(BuildTarget.StandaloneWindows64)]
        public void GetAndSetSourceLocation()
        {
            TestHelper.InitialiseNVAR(0);
            {
                NVAR.Context context = TestHelper.CreateNVARContext();
                {
                    // Create NVAR sound source
                    NVAR.Source source;
                    NVAR.Status status = NVAR.CreateSource(context, NVAR.EffectPreset.High, out source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreateSource() failed");

                    // Set source location
                    Vector3 location = new Vector3(1, 2, 3);
                    status = NVAR.SetSourceLocation(source, location);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarSetSourceLocation() failed");

                    // Get source location in NVAR
                    Vector3 nvarLocation;
                    status = NVAR.GetSourceLocation(source, out nvarLocation);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetSourceLocation() failed");

                    // Ensure NVAR source location is as expected
                    Assert.AreEqual(location, nvarLocation, "NVAR source location not expected value");

                    // Destroy NVAR sound source
                    status = NVAR.DestroySource(source);
                    Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroySource() failed");
                }
                TestHelper.DestroyNVARContext(context);
            }
            TestHelper.FinaliseNVAR();
        }
    }

    internal static class TestHelper
    {
        #region Internal Functions

        internal static void InitialiseNVAR(int a_flags)
        {
            NVAR.Status status = NVAR.Initialize(a_flags);

            // Ensure NVAR was intialised successfully
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarInitialize() failed");
        }

        internal static void FinaliseNVAR()
        {
            NVAR.Status status = NVAR.Finalize();
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarFinalize() failed");
        }

        internal static NVAR.Context CreateNVARContext()
        {
            // Get number of CUDA devices
            int deviceCount;
            NVAR.Status status = NVAR.GetDeviceCount(out deviceCount);

            // Ensure the request for device count in NVAR succeeded
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarGetDeviceCount() failed");
            // Ensure we have at least one CUDA device on this machine to perform this unit test
            if (deviceCount <= 0)
            {
                Assert.Fail("No CUDA devices available on this machine");
            }

            return CreateNVARContext(0);
        }

        internal static NVAR.Context CreateNVARContext(int a_deviceID)
        {
            // Define NVAR context properties
            string contextName          = "Test Context";
            NVAR.EffectPreset preset    = NVAR.EffectPresetDefault;
            int deviceNum               = 0;

            // Create NVAR context
            NVAR.Context context;
            NVAR.Status status = NVAR.Create(out context, contextName, preset, deviceNum);

            // Ensure NVAR context creation succeded
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarCreate() failed");

            return context;
        }

        internal static void DestroyNVARContext(NVAR.Context a_context)
        {
            // Destroy NVAR context
            NVAR.Status status = NVAR.Destroy(a_context);

            // Ensure NVAR context was successfully destroyed
            Assert.AreEqual(NVAR.Status.Success, status, "Call to nvarDestroy() failed");
        }

        #endregion
    }
}

namespace NVIDIA.VRWorksAudio.Internal.Tests.NestedTypes
{
    [TestFixture]
    public sealed class ContextTests
    {
        #region Constructor Tests

        [Test]
        [Category("Constructor Test")]
        [Description("Tests normal/expected usage of the full constructor")]
        public void FullConstructor()
        {
            IntPtr pointer = new IntPtr(10);
            NVAR.Context context = new NVAR.Context(pointer);

            Assert.AreEqual(pointer, context.pointer);
        }

        #endregion

        #region Property Tests

        [Test]
        [Category("Property Test")]
        [Description("Tests normal/expected usage of property 'isNull'")]
        public void IsNull()
        {
            NVAR.Context context = new NVAR.Context();

            // Test property with what is considered to be a NULL pointer
            context = new NVAR.Context(IntPtr.Zero);
            Assert.IsTrue(context.isNull);

            // Test property with what is considered to be a valid pointer
            context = new NVAR.Context(new IntPtr(10));
            Assert.IsFalse(context.isNull);
        }

        #endregion
    }

    [TestFixture]
    public sealed class MaterialTests
    {
        #region Constructor Tests

        [Test]
        [Category("Constructor Test")]
        [Description("Tests normal/expected usage of the full constructor")]
        public void FullConstructor()
        {
            IntPtr pointer = new IntPtr(10);
            NVAR.Material material = new NVAR.Material(pointer);

            Assert.AreEqual(pointer, material.pointer);
        }

        #endregion

        #region Property Tests

        [Test]
        [Category("Property Test")]
        [Description("Tests normal/expected usage of property 'isNull'")]
        public void IsNull()
        {
            NVAR.Material context = new NVAR.Material();

            // Test property with what is considered to be a NULL pointer
            context = new NVAR.Material(IntPtr.Zero);
            Assert.IsTrue(context.isNull);

            // Test property with what is considered to be a valid pointer
            context = new NVAR.Material(new IntPtr(50));
            Assert.IsFalse(context.isNull);
        }

        #endregion
    }

    [TestFixture]
    public sealed class MeshTests
    {
        #region Constructor Tests

        [Test]
        [Category("Constructor Test")]
        [Description("Tests normal/expected usage of the full constructor")]
        public void FullConstructor()
        {
            IntPtr pointer = new IntPtr(10);
            NVAR.Mesh mesh = new NVAR.Mesh(pointer);

            Assert.AreEqual(pointer, mesh.pointer);
        }

        #endregion

        #region Property Tests

        [Test]
        [Category("Property Test")]
        [Description("Tests normal/expected usage of property 'isNull'")]
        public void IsNull()
        {
            NVAR.Mesh mesh = new NVAR.Mesh();

            // Test property with what is considered to be a NULL pointer
            mesh = new NVAR.Mesh(IntPtr.Zero);
            Assert.IsTrue(mesh.isNull);

            // Test property with what is considered to be a valid pointer
            mesh = new NVAR.Mesh(new IntPtr(10));
            Assert.IsFalse(mesh.isNull);
        }

        #endregion
    }

    [TestFixture]
    public sealed class SourceTests
    {
        #region Constructor Tests

        [Test]
        [Category("Constructor Test")]
        [Description("Tests normal/expected usage of the full constructor")]
        public void FullConstructor()
        {
            IntPtr pointer = new IntPtr(10);
            NVAR.Context context = new NVAR.Context();
            NVAR.Source source = new NVAR.Source(pointer, context);

            Assert.AreEqual(pointer, source.pointer);
            Assert.AreEqual(context, source.context);
        }

        #endregion

        #region Property Tests

        [Test]
        [Category("Property Test")]
        [Description("Tests normal/expected usage of property 'isNull'")]
        public void IsNull()
        {
            NVAR.Context context    = new NVAR.Context();
            NVAR.Source source      = new NVAR.Source();

            // Test property with what is considered to be a NULL pointer
            source = new NVAR.Source(IntPtr.Zero, context);
            Assert.IsTrue(source.isNull);

            // Test property with what is considered to be a valid pointer
            source = new NVAR.Source(new IntPtr(10), context);
            Assert.IsFalse(source.isNull);
        }

        #endregion
    }
}
