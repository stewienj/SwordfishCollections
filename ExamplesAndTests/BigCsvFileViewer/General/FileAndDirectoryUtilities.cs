using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

namespace Swordfish.NET.General {
  public class Disposable : IDisposable {
    private Action _dispose;
    public Disposable(Action dispose = null) {
      _dispose = dispose;
    }
    void IDisposable.Dispose() {
      if(_dispose != null) {
        _dispose();
      }
    }
  }


  public static class FileAndDirectoryUtilities {

    private class AnonDisposableReader : StreamReader {
      Action _dispose;
      public AnonDisposableReader(Stream stream, Action dispose)
        : base(stream) {
        _dispose = dispose;
      }

      protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if(disposing) {
          _dispose();
        }
      }
    }

    private class AnonDisposableBinaryReader : BinaryReader {
      Action _dispose;
      public AnonDisposableBinaryReader(Stream stream, Action dispose)
        : base(stream) {
        _dispose = dispose;
      }

      protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if(disposing) {
          _dispose();
        }
      }
    }

    private class AnonDisposableWriter : StreamWriter {
      Action _dispose;
      public AnonDisposableWriter(Stream stream, Action dispose)
        : base(stream) {
        _dispose = dispose;
      }

      protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if(disposing) {
          _dispose();
        }
      }
    }

    private class AnonDisposableBinaryWriter : BinaryWriter {
      Action _dispose;
      public AnonDisposableBinaryWriter(Stream stream, Action dispose)
        : base(stream) {
        _dispose = dispose;
      }

      protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if(disposing) {
          _dispose();
        }
      }
    }
    
    public static StreamReader StreamReaderBuffered(string filename, int bufferSize = 65536) {
      FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize);
      AnonDisposableReader streamReader = new AnonDisposableReader(fileStream, () => {
        fileStream.Dispose();
      });
      return streamReader;
    }

    public static StreamWriter StreamWriterBuffered(string filename, int bufferSize = 65536) {
      FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize);
      AnonDisposableWriter streamWriter = new AnonDisposableWriter(fileStream, () => {
        fileStream.Dispose();
      });
      return streamWriter;
    }

    public static BinaryReader BinaryReaderBuffered(string filename, int bufferSize = 65536) {
      FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize);
      AnonDisposableBinaryReader binaryReader = new AnonDisposableBinaryReader(fileStream, () => {
        fileStream.Dispose();
      });
      return binaryReader;
    }


    public static BinaryWriter BinaryWriterBuffered(string filename, int bufferSize = 65536) {
      FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize);
      AnonDisposableBinaryWriter binaryWriter = new AnonDisposableBinaryWriter(fileStream, () => {
        fileStream.Dispose();
      });
      return binaryWriter;
    }

    public static string SidToAccountName(IdentityReference sidRef) {

      try {
        IdentityReference account = sidRef.Translate(typeof(NTAccount));
        return account.ToString() + " (" + sidRef.Value + ")";
      } catch(Exception) {
      }

      try {
        if(!(sidRef is SecurityIdentifier)) {
          return sidRef.Value;
        }

        byte[] Sid = new byte[128];

        ((SecurityIdentifier)sidRef).GetBinaryForm(Sid, 0);

        const int NO_ERROR = 0;
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        StringBuilder name = new StringBuilder();
        uint cchName = (uint)name.Capacity;
        StringBuilder referencedDomainName = new StringBuilder();
        uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
        Win32.SidNameUse sidUse;
        // Sid for BUILTIN\Administrators
        //byte[] Sid = new byte[] { 1, 2, 0, 0, 0, 0, 0, 5, 32, 0, 0, 0, 32, 2 };

        int err = NO_ERROR;
        if(!Win32.LookupAccountSid(null, Sid, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out sidUse)) {
          err = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
          if(err == ERROR_INSUFFICIENT_BUFFER) {
            name.EnsureCapacity((int)cchName);
            referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
            err = NO_ERROR;
            if(!Win32.LookupAccountSid(null, Sid, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out sidUse))
              err = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
          }
        }
        if(err == 0)
          return name.ToString() + " (" + sidRef.Value + ")";
      } catch(Exception) {
      }
      return sidRef.Value;
    }

    public static bool CheckReadAccess(string directoryName, List<string> diagnostics = null) {
      return CheckReadAccess(new DirectoryInfo(directoryName), diagnostics);
    }

    /// <summary>
    /// Checks the read access and writes the status out to the console
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public static bool CheckReadAccessConsole(string directoryName) {
      try {
        bool exists = Directory.Exists(directoryName);
        if(exists) {
          List<string> diagnostics = new List<string>();
          bool hasAccess = FileAndDirectoryUtilities.CheckReadAccess(directoryName, diagnostics);
          if(!hasAccess) {
            foreach(string line in diagnostics) {
              Console.WriteLine(line);
            }
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = hasAccess ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("Checking read access to Directory: {0}", hasAccess ? "PASSED" : "FAILED");
          }
          return hasAccess;
        } else {
          Console.ForegroundColor = ConsoleColor.Black;
          Console.BackgroundColor = exists ? ConsoleColor.Green : ConsoleColor.Red;
          Console.WriteLine("Checking Directory Exists:         {0}", exists ? "PASSED" : "FAILED");
          return false;
        }
      } catch(Exception e) {
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.WriteLine("Got Error accessing Terrain Tile Directory");
        return false;
      } finally {
        Console.ResetColor();
      }
    }

    /// <summary>
    /// Checks that a directory can be read by the current process
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public static bool CheckReadAccess(DirectoryInfo directory, List<string> diagnostics = null) {
      if (!directory.Exists) return false;

      System.Security.Principal.WindowsIdentity user = System.Security.Principal.WindowsIdentity.GetCurrent();
      System.Security.Principal.WindowsPrincipal principal = new WindowsPrincipal(user);

      // Get the collection of authorization rules that apply to the current directory
      AuthorizationRuleCollection acl = directory.GetAccessControl().GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

      if(diagnostics != null) {
        diagnostics.Add("Checking process group membership...");
        foreach(IdentityReference idref in user.Groups) {
          diagnostics.Add("- " + SidToAccountName(idref));
        }
        diagnostics.Add("Now testing access rules...");
      }
      // These are set to true if either the allow read or deny read access rights are set
      bool allowRead = false;
      bool denyRead = false;

      for(int x = 0; x < acl.Count; x++) {
        FileSystemAccessRule currentRule = (FileSystemAccessRule)acl[x];
        // If the current rule applies to the current user
        if(user.User.Equals(currentRule.IdentityReference) || principal.IsInRole((SecurityIdentifier)currentRule.IdentityReference)) {
          if(diagnostics != null) {
            diagnostics.Add("Current process is a member of " + SidToAccountName(currentRule.IdentityReference));
          }
          if(currentRule.AccessControlType.Equals(AccessControlType.Deny)) {
            if((currentRule.FileSystemRights & FileSystemRights.Read) == FileSystemRights.Read) {
              denyRead = true;
              if(diagnostics != null) {
                diagnostics.Add("Read access explicitly denied");
              }
            }
          } else if(currentRule.AccessControlType.Equals(AccessControlType.Allow)) {
            if((currentRule.FileSystemRights & FileSystemRights.Read) == FileSystemRights.Read) {
              allowRead = true;
              if(diagnostics != null) {
                diagnostics.Add("Read access explicitly granted");
              }
            }
          }
        } else if(diagnostics != null) {
          diagnostics.Add("Current process is not a member of " + SidToAccountName(currentRule.IdentityReference));
        }

      }
      if(denyRead)
        return false;
      if(allowRead)
        return true;

      // Shouldn't get to here
      if(diagnostics != null) {
        diagnostics.Add("No Read access rules found for current user or role");
        diagnostics.Add("Read access not explicity granted");
      }

      return false;
    }
  }
}
