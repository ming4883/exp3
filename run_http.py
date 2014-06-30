import SimpleHTTPServer
import SocketServer
import os

os.chdir("www")

PORT = 8080

Handler = SimpleHTTPServer.SimpleHTTPRequestHandler

httpd = SocketServer.TCPServer(("", PORT), Handler)

print "Serving ", os.getcwd()," at port", PORT
httpd.serve_forever()
