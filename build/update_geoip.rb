require 'fileutils'
require 'open-uri'
require 'net/http'
include Net

URL = "http://geolite.maxmind.com/download/geoip/database/GeoLiteCountry/GeoIP.dat.gz"
FILE = "GeoIP.dat"
FILE_GZ = FILE + ".gz"

def unpack_file file; system "7za x \"#{file}\""; end
def download_file url, file
	File.open(file, "wb") do |saved_file|
	  open(url, 'rb') do |read_file|
	    saved_file.write(read_file.read)
	  end
	end
end

download_file URL, FILE_GZ
unpack_file FILE_GZ
FileUtils.cp(FILE, File.join("../src/tools/config/", FILE), preserve: true)
FileUtils.mv(FILE, File.join("../src/Six.Foundation.Resources/config/", FILE))
FileUtils.rm(FILE_GZ)
