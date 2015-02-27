#
# Generated Makefile - do not edit!
#
# Edit the Makefile in the project folder instead (../Makefile). Each target
# has a -pre and a -post target defined where you can add customized code.
#
# This makefile implements configuration specific macros and targets.


# Environment
MKDIR=mkdir
CP=cp
GREP=grep
NM=nm
CCADMIN=CCadmin
RANLIB=ranlib
CC=gcc
CCC=g++
CXX=g++
FC=gfortran
AS=as

# Macros
CND_PLATFORM=GNU-Linux-x86
CND_DLIB_EXT=so
CND_CONF=Debug
CND_DISTDIR=dist
CND_BUILDDIR=build

# Include project Makefile
include Makefile

# Object Directory
OBJECTDIR=${CND_BUILDDIR}/${CND_CONF}/${CND_PLATFORM}

# Object Files
OBJECTFILES= \
	${OBJECTDIR}/_ext/2128269649/String.o \
	${OBJECTDIR}/_ext/2128269649/Thread.o \
	${OBJECTDIR}/_ext/2128269649/dllmain.o \
	${OBJECTDIR}/main.o


# C Compiler Flags
CFLAGS=

# CC Compiler Flags
CCFLAGS=-pthread
CXXFLAGS=-pthread

# Fortran Compiler Flags
FFLAGS=

# Assembler Flags
ASFLAGS=

# Link Libraries and Options
LDLIBSOPTIONS=

# Build Targets
.build-conf: ${BUILD_SUBPROJECTS}
	"${MAKE}"  -f nbproject/Makefile-${CND_CONF}.mk ${CND_DISTDIR}/${CND_CONF}/${CND_PLATFORM}/utilitiestest

${CND_DISTDIR}/${CND_CONF}/${CND_PLATFORM}/utilitiestest: ${OBJECTFILES}
	${MKDIR} -p ${CND_DISTDIR}/${CND_CONF}/${CND_PLATFORM}
	${LINK.cc} -o ${CND_DISTDIR}/${CND_CONF}/${CND_PLATFORM}/utilitiestest ${OBJECTFILES} ${LDLIBSOPTIONS}

${OBJECTDIR}/_ext/2128269649/String.o: ../../../cpp/Utils/Utilities/String.cpp 
	${MKDIR} -p ${OBJECTDIR}/_ext/2128269649
	${RM} "$@.d"
	$(COMPILE.cc) -g -I../../../cpp/Utils/ -MMD -MP -MF "$@.d" -o ${OBJECTDIR}/_ext/2128269649/String.o ../../../cpp/Utils/Utilities/String.cpp

${OBJECTDIR}/_ext/2128269649/Thread.o: ../../../cpp/Utils/Utilities/Thread.cpp 
	${MKDIR} -p ${OBJECTDIR}/_ext/2128269649
	${RM} "$@.d"
	$(COMPILE.cc) -g -I../../../cpp/Utils/ -MMD -MP -MF "$@.d" -o ${OBJECTDIR}/_ext/2128269649/Thread.o ../../../cpp/Utils/Utilities/Thread.cpp

${OBJECTDIR}/_ext/2128269649/dllmain.o: ../../../cpp/Utils/Utilities/dllmain.cpp 
	${MKDIR} -p ${OBJECTDIR}/_ext/2128269649
	${RM} "$@.d"
	$(COMPILE.cc) -g -I../../../cpp/Utils/ -MMD -MP -MF "$@.d" -o ${OBJECTDIR}/_ext/2128269649/dllmain.o ../../../cpp/Utils/Utilities/dllmain.cpp

${OBJECTDIR}/main.o: main.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} "$@.d"
	$(COMPILE.cc) -g -I../../../cpp/Utils/ -MMD -MP -MF "$@.d" -o ${OBJECTDIR}/main.o main.cpp

# Subprojects
.build-subprojects:

# Clean Targets
.clean-conf: ${CLEAN_SUBPROJECTS}
	${RM} -r ${CND_BUILDDIR}/${CND_CONF}
	${RM} ${CND_DISTDIR}/${CND_CONF}/${CND_PLATFORM}/utilitiestest

# Subprojects
.clean-subprojects:

# Enable dependency checking
.dep.inc: .depcheck-impl

include .dep.inc
