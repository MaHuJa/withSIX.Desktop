from mikero@norfolk.nf

DePbo.dll is the 'driver' behind Mikero's tools. It is intended to be a non-reinventing-the-wheel service so that any application can
access one, well proven, decompression algorithm (eg), rather than re-writing or pasting the source code again and again and again into different exe's.

While the name suggests a pbo only driver, in fact, depbo has expanded over the years to much greater functionality. Rapification, to name one.

A far far far better name would now be 'BisDll', and in fact, internally, it is known by that name. Externally however, after a decade of usage, people
are used to 'DePbo.dll' so it stays that way.


If you require source, or an SDK, or Visual basic wrappers to this dll. they are available at

www.ofpec.com
http://andrew.nf/ofp/tools
http://linux-sxs.org  (linux .so version intended for server apps)
http://dev-heaven.net/projects/list_files/mikero-pbodll

or contact me at above email (if you're brave enough to face my anti-spammer)


A single Depbo.dll should be placed in your windows\system32 folder for each and any tool.exe to gain access to one, common driver.

For win64 machines, do the Wow


Alternatively, simply place the dll in the same folder as the exe.

It is garanteed that THIS version of the dll will ALWAYS be compatible with previous versions. You need, one, and one only copy.


**********************************************************************************************************************************************
the depbo.lib is NOT required for tool use. Dont even bother extracting it unless you intended programming via visual basic and using the api
**********************************************************************************************************************************************

---

After 40 years in the industry, this author is dedicated to backward compatibility. 

Any upgrade to dll (which is frequent) is garanteed to be compatible with existing exes and applications, OR, a new exe is provided. No matter how ancient the
exe is, someone, somewhere, is using it every day and cannot live without it.




enjoy



ALL exes exhibit similar behaviour in command line usage.

nameof.exe [-options] sourcefile/Folder[.ext]/orList[.lst] [[outfile/Folder[.ext]]

Destination specs

where (an often optional) destination can be specified, the name can be either a file or a path or both

In general, the resulting filename follows the exe's specification extension such as p3d, cpp. rvmat. Obviously, dependent on context.


