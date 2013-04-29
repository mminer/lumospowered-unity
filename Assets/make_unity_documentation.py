#!/usr/bin/python
import os
import sys
from subprocess import call


def copy_files(root, destination):
    """Copies files to a destination"""
    command = "rsync -r -v --include '*/' --include '*.cs' --exclude '*' %s %s" % (root, dest_root)
    call(command, shell=True)


def merge_social_gui(social_gui, root):
    """Merges the social gui partial classes"""  
    # Remove the closing brace from LumosSocialGUI before merge
    remove_closing_brace(social_gui)  

    with open(social_gui, 'a') as fout:  
        # Achievements
        achievements = '%s/LumosAchievementsGUI.cs' % (root)
        merge_file(achievements, 5, fout)
        
        # Login
        login = '%s/LumosLoginGUI.cs' % (root)
        merge_file(login, 5, fout)
        
        # Leaderboards
        leaderboards = '%s/LumosLeaderboardsGUI.cs' % (root)
        merge_file(leaderboards, 7, fout)
        
        # Settings
        settings = '%s/LumosSettingsGUI.cs' % (root)
        merge_file(settings, 6, fout)
        
        # Registration
        registration = '%s/LumosRegistrationGUI.cs' % (root)
        merge_file(registration, 5, fout)
        
        # Profile
        profile = '%s/LumosProfileGUI.cs' % (root)
        merge_file(profile, 7, fout)
        
        # Forgot Password
        forgot = '%s/LumosForgotPasswordGUI.cs' % (root)
        merge_file(forgot, 5, fout)
        
        # Add closing brace back in
        fout.write('\n}')


def merge_lumos(lumos, root):
    """Merges the core Lumos partial classes."""
    # Remove the closing brace before merge
    remove_closing_brace(lumos)
    
    with open(lumos, 'a') as fout:  
        debug = '%s/LumosDebug.cs' % (root)
        merge_file(debug, 9, fout)

        # Add closing brace back in
        fout.write('\n}')


def remove_closing_brace(file):
    """Removes the last line of a file"""
    # Remove line from file
    command ="sed -i.bak '$d' %s" % (file)
    call(command, shell=True)
    
    # Delete backup file
    remove = 'rm %s.bak' % (file)
    call(remove, shell=True)
        

def merge_file(file, skip_lines, file_out):
    """Merges a Social GUI file to the main LumosSocialGUI file"""
    with open(file, 'r') as file_in:
        current_line = 0
        lines = file_in.readlines()
        
        for line in lines:
            current_line += 1
            
            # Skip import & constructor lines
            if current_line <= skip_lines:
                continue
            
            # Skip the closing brace
            if len(lines) - 1 == current_line:
                continue
            
            file_out.write(line)
    
    # Remove the original file
    command ="rm %s" % (file)
    call(command, shell=True)


def create_documentation(root, unity_dll):
    """Creates XML files for all .cs files in the given directory"""
    for root, subFolders, files in os.walk(root):
        for file in files:
            # Skip this file type
            if '.DS_Store' in file:
                continue
            
            # Get the full file path
            path = os.path.join(root, file)
            
            # Run the mcs xml doc command
            command = 'mcs %s %s -target:library /doc:Docs/%s.xml' % (path, unity_dll, file)
            call(command, shell=True)


if __name__ == '__main__':
    root = 'Scripts'
    dest_root = '/Users/Dagoth/Desktop/UnityLumosScripts'
    gui_dir = '%s/Scripts/Powerups/Social/GUI' % (dest_root)
    lumos_dir = '%s/Scripts' % (dest_root)
    lumos = '%s/Lumos.cs' % (lumos_dir)
    social_gui = '%s/LumosSocialGUI.cs' % (gui_dir)
    unity_dll = '-r:/Applications/Unity/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll'
    
    # Copy scripts before modifying them
    copy_files(root, dest_root)
    
    # Merge partial social GUI classes
    merge_social_gui(social_gui, gui_dir)
    
    # Merge core Lumos partial classes
    merge_lumos(lumos, lumos_dir)
    
    # Create XML documentation of the resulting .cs files
    create_documentation(dest_root, unity_dll)
