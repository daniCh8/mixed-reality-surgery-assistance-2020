import nrrd
import sys

file = sys.argv[1]
data, headers = nrrd.read(file)
headers['encoding'] = 'gzip'

data = data.astype('float128')
data = ((data - data.min()) / (data.max() - data.min())).astype('<f4')

nrrd.write(file, data, headers)
