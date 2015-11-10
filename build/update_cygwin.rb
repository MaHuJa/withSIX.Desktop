require 'fileutils'
path = '../src/tools/cygwin/bin'
cyg_path = 'P:/cygwin/bin'
# cygasn1-8.dll
# cyggssapi-3.dll
# cygheimbase-1.dll
# cygheimntlm-0.dll
# cyghx509-5.dll
# cygroken-18.dll
# cygwind-0.dll

%w[
cygcom_err-2.dll
cygcrypt-0.dll
cygcrypto-1.0.0.dll
cygk5crypto-3.dll
cyggcc_s-1.dll
cyggssapi_krb5-2.dll
cygexpat-1.dll
cygiconv-2.dll
cygintl-8.dll
cygkrb5-3.dll
cygkrb5support-0.dll
cygncursesw-10.dll
cygreadline7.dll
cygsqlite3-0.dll
cygssl-1.0.0.dll
cygssp-0.dll
cygstdc++-6.dll
cygwin1.dll
cygz.dll
lftp.exe
ssh.exe
rsync.exe
zsync.exe
zsyncmake.exe
].each do |entry|
	puts "Copying #{entry}"
	FileUtils.cp(File.join(cyg_path, entry), File.join(path, entry), :preserve => true)
end

puts "Done!"
gets
