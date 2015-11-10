require 'fileutils'
path = '../src/tools/mingw/bin'
cyg_path = 'C:/mingw/msys/1.0/bin'

%w[msys-1.0.dll
msys-crypt-0.dll
msys-crypto-1.0.0.dll
msys-iconv-2.dll
msys-intl-8.dll
msys-minires.dll
msys-popt-0.dll
msys-regex-1.dll
msys-ssl-1.0.0.dll
msys-termcap-0.dll
msys-z.dll
rsync.exe
ssh.exe
ssh-add.exe
ssh-agent.exe
ssh-keygen.exe
ssh-keyscan.exe
tar.exe
].each do |entry|
	puts "Copying #{entry}"
	FileUtils.cp(File.join(cyg_path, entry), File.join(path, entry), :preserve => true)
end

puts "Done!"
gets
