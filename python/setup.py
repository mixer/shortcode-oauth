import sys
from setuptools import setup, find_packages

if sys.version_info < (3, 5):
    raise Exception("mixer_shortcode makes use of asyncio, async, and"
                    "await, and therefore requires Python >= 3.5.x")

setup(
    name='mixer_shortcode',
    version='1.0.0',
    description='Mixer OAuth shortcode client',
    classifiers=[
        'Development Status :: 2 - Pre-Alpha',
        'Programming Language :: Python :: 3.5',
        'Programming Language :: Python :: 3.6',
        'License :: OSI Approved :: MIT License',
        'Topic :: Games/Entertainment',
        'Topic :: Software Development',
        'Topic :: Software Development :: Libraries'
    ],
    author='Connor Peet',
    author_email='connor@xbox.com',
    url='https://github.com/mixer/shortcode-oauth',
    license='MIT',
    packages=find_packages(exclude=['tests']),
    install_requires=['aiohttp>=3.4.1'],
    include_package_data=True,
)
