# define name of installer
!include "MUI2.nsh"

# define version information
!define PRODUCT_VERSION "1.0.0.0"
!define VERSION "1.0.0.0"
!define PRODUCT_NAME "DatChecker"
!define COMPANY "Frank Refol" 
!define DESCRIPTION "Checks if a concordance DAT file is valid"
!define COPYRIGHT "© Frank Refol 2020. All rights reserved."
!define MAIN_EXECUTABLE "DatChecker.exe"
!define MAIN_EXECUTABLE_CONFIG "DatChecker.exe.config"
!define RELEASE_DIR "..\DatChecker\bin\Release\"

OutFile "DatChecker_setup.exe"

Name "${PRODUCT_NAME}"
Caption "${PRODUCT_NAME}"
ShowInstDetails show

VIProductVersion "${PRODUCT_VERSION}"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "LegalCopyright" "${COPYRIGHT}"
VIAddVersionKey "FileDescription" "${DESCRIPTION}"
VIAddVersionKey "ProductName" "${PRODUCT_NAME}"

# define installation directory
#InstallDir "$PROGRAMFILES\$(^Name)"
InstallDir "$LocalAppData\$(^Name)"
 
# For removing Start Menu shortcut in Windows 7
RequestExecutionLevel admin
 
;--------------------------------
;Interface Configuration
!define MUI_PAGE_HEADER_TEXT "$(^Name) Setup"
!define MUI_DIRECTORYPAGE_TEXT_TOP "Select a Destination Folder."
!define MUI_INSTFILESPAGE_FINISHHEADER_TEXT "$(^Name) Installed"
!define MUI_FINISHPAGE_RUN "$INSTDIR\${MAIN_EXECUTABLE}"
!define MUI_FINISHPAGE_RUN_FUNCTION fnc_CI_Run
!define MUI_FINISHPAGE_RUN_TEXT "Launch $(^Name)"
!define MUI_FINISHPAGE_TITLE "Setup Complete"
!define MUI_FINISHPAGE_TEXT "$(^Name) is now installed. Click Close to complete setup."
!define MUI_FINISHPAGE_BUTTON "Close"
!define MUI_UNCONFIRMPAGE_TEXT_TOP "Uninstalling $(^Name)"
!define MUI_UNCONFIRMPAGE_TEXT_LOCATION "Uninstalling $(^Name)"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_UNFINISHPAGE_NOAUTOCLOSE

# Pages
!insertmacro MUI_PAGE_DIRECTORY

!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

; Create the shared function.
!macro MYMACRO un
  Function ${un}killapp
	StrCpy $0 "${MAIN_EXECUTABLE}"
	DetailPrint "Searching for processes called '$0'"
	KillProc::FindProcesses
	StrCmp $1 "-1" wooops
	DetailPrint "-> Found $0 processes"
 
   StrCmp $0 "0" completed
   Sleep 1500
 
   StrCpy $0 "${MAIN_EXECUTABLE}"
   DetailPrint "Killing all processes called '$0'"
   KillProc::KillProcesses
   StrCmp $1 "-1" wooops
   DetailPrint "-> Killed $0 processes, failed to kill $1 processes"
   Sleep 1500
 
   Goto completed
 
   wooops:
   DetailPrint "-> Error: Something went wrong :-("
   Abort

   completed:
   DetailPrint "Everything went okay :-D"
  FunctionEnd
!macroend

!macro SetConfigValue ConfigFile Key Value
 
	${nsisXML->OpenXML}	"${ConfigFile}"
	DetailPrint "Setting /configuration/appSettings/add[@key='${Key}'] value to ${Value}"
	${nsisXML->SetElementAttr} "/configuration/appSettings/add[@key='${Key}']" "value" "${Value}"
	${nsisXML->Release} "${ConfigFile}"
	DetailPrint "Successfully saved ${ConfigFile}"
	   
!macroend

Function .onInit
	DetailPrint "Checking if $(^Name) is installed."
	#ReadRegStr $R0 HKLM "SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" "UninstallString"
	ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" "UninstallString"
	DetailPrint "Uninstall string read is $R0"
	StrCmp $R0 "" NotInstalled
	MessageBox MB_YESNO|MB_TOPMOST "$(^Name) is already installed. Uninstall?" IDYES Yes IDNO No
	No:
		DetailPrint "$(^Name) is installed. Quitting install."
		Quit
	Yes:
		DetailPrint "Uninstalling $(^Name)."
		ExecWait $R0
 	NotInstalled:
	DetailPrint "$(^Name) not installed. Continuing with installation."

	# start install
FunctionEnd

# Executes Main application
Function fnc_CI_Run
	SetOutPath "$INSTDIR"
	ExecShell "" "$SMPROGRAMS\$(^Name)\$(^Name).lnk"	
FunctionEnd

!define LVM_GETITEMCOUNT 0x1004
!define LVM_GETITEMTEXTA 0x102D
!define LVM_GETITEMTEXTW 0x1073
!if "${NSIS_CHAR_SIZE}" > 1
!define LVM_GETITEMTEXT ${LVM_GETITEMTEXTW}
!else
!define LVM_GETITEMTEXT ${LVM_GETITEMTEXTA}
!endif
 
; Insert function as an installer and uninstaller function.
!insertmacro MYMACRO ""
!insertmacro MYMACRO "un."

# start default section
Section "Installation"
	# start install
	call killapp
    # set the installation directory as the destination for the following actions
	DetailPrint "Setting installation folder to $INSTDIR"
    SetOutPath $INSTDIR

    # create the uninstaller
	DetailPrint "Creating uninstall.exe"
    WriteUninstaller "$INSTDIR\uninstall.exe"
	
	# files to copy
	DetailPrint "Copying files."
	File /r ${RELEASE_DIR}\*.*
	
	# Add add/remove entry
	DetailPrint "Creating uninstall entry in registry."
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" \
		"DisplayName" "$(^Name)"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" \
		"UninstallString" "$\"$INSTDIR\uninstall.exe$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" \
		"DisplayIcon" "$\"$INSTDIR\${MAIN_EXECUTABLE}$\""	
	
SectionEnd

Section "Start Menu Shortcuts"
	DetailPrint "Creating shortcuts."
	CreateDirectory "$SMPROGRAMS\$(^Name)"
	CreateShortCut "$SMPROGRAMS\$(^Name)\$(^Name).lnk" "$INSTDIR\${MAIN_EXECUTABLE}"
    CreateShortCut "$SMPROGRAMS\$(^Name)\Uninstall.lnk" "$INSTDIR\uninstall.exe"
SectionEnd

 
# uninstaller section start
Section "uninstall"

	call un.killapp
    # first, delete the uninstaller
	DetailPrint "Delete $INSTDIR\uninstall.exe"
    Delete "$INSTDIR\uninstall.exe"

    # second, remove the link from the start menu
	DetailPrint "Deleting shortcuts."
	Delete "$SMPROGRAMS\Uninstall.lnk"
    Delete "$SMPROGRAMS\$(^Name)\$(^Name).lnk"
	RMDir "$SMPROGRAMS\$(^Name)"
	

	# fourth, delete installation folder
	DetailPrint "Installation folder and files."
	RMDir /r  "$INSTDIR"
	
	# Remove add/remove registry entry
	DetailPrint "Deleting uninstall entry from registry."
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
	DetailPrint "Deleting user settings from registry."
	DeleteRegKey HKCU "SOFTWARE\Frank Refol\$(^Name)"
 
# uninstaller section end
SectionEnd