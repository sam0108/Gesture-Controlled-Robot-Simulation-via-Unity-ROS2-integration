import sys
if sys.prefix == '/usr':
    sys.real_prefix = sys.prefix
    sys.prefix = sys.exec_prefix = '/home/esmmam/robotic_final/Test/robotics-final/install/final_package'
