# UNotes

<img src='https://raw.githubusercontent.com/rstecca/UNotes/master/README/UNotesWithComments.png' alt='UNotes Full Screen Shot With Comments'></img>

# License 

<a rel="license" href="http://creativecommons.org/licenses/by-nd/4.0/"><img alt="Creative Commons Licence" style="border-width:0" src="https://i.creativecommons.org/l/by-nd/4.0/88x31.png" /></a><br />This work is licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nd/4.0/">Creative Commons Attribution-NoDerivatives 4.0 International License</a>.

# Donations

If you find UNotes useful then consider sending some appreciation in form of currency that I will probably swap for more coffee :coffee: to keep cracking problems that bother you.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=H8K4S4MLJSQ7Y)

# What is it?

UNotes is a lightweight productivity tool that helps you accelerate your workflow by bringing the power of sticky notes into Unity.
A great, colorful way to keep track of your tasks.
Stick a note on your game objects in the editor and type in your goals. See all of your notes in one place and go to the object they point to in one single click!
Leaves no traces when you build.

UNotes gives its best against complex scene hierarchies which make finding a specific GameObject particularly obnoxious but every project, even the simplest, can take advantage of UNotes.

**Please note: in addition to this documentation we included a scene called “Tutorial” that guides you through all features. Highly recommended.**

<img src='https://raw.githubusercontent.com/rstecca/UNotes/master/README/UnotesScreenShot00005.png' alt='UNotes Full Screen Shot'></img>

# Install

## From Asset Store

UNotes 2.0.0 is available on the Unity's Asset Store for free at https://www.assetstore.unity3d.com/en/#!/content/51149

## From his repository

After you've cloned or downloaded this repository, copy the whole UNotes folder contained in Assets (https://github.com/rstecca/UNotes/tree/master/UNotes/Assets) and paste it into your project's Assets folder.

# How to use it

Once you acquired and imported the package, UNotes is really easy to use.
Works with Unity 5.x, Unity 2017.x and Unity 2018.x.

## Attach a UNote to a GameObject

* Select the GameObject you want to stick a UNote to, right click and select UNotes > Add Note.
* Dock the UNote Editor somewhere in your Layout.
* Type your tasks, notes, todos or curses in your new note. Foul language is encouraged.
* Select the note’s style by expanding the Options menu in the UNote window.

<img src='https://raw.githubusercontent.com/rstecca/UNotes/master/README/UnotesScreenShot00001.png' alt='UNotes Attach Note' width='50%'></img>

## Styles

Font size, weight, background and foreground colors can be changed.

<img src='https://raw.githubusercontent.com/rstecca/UNotes/master/README/UnotesScreenShot00004.png' alt='UNotes Nostalgic Style' width='50%'></img>

The default style can be set by going to Window > UNotes > Options.

## Hierarchy View

In the Hierarchy View, styled dots will indicate which GameObjects have a note attached.

<img src='https://raw.githubusercontent.com/rstecca/UNotes/master/README/UnotesScreenShot00007.png' alt='UNotes Attach Note' width='50%'></img>

## Browsing all your UNotes

UNotes’ most valuable feature is the UNotes Block. Whenever you create a new UNote, this will immediately appear in the UNotes Block. Here you can

* browse all the UNotes that you sticked around your scene.
* reorder them by dragging them up or down
* search through all UNotes by entering your text in the search field at the top
* and, most importantly, select the GameObject the UNote belongs to with one single click.

<img src='https://raw.githubusercontent.com/rstecca/UNotes/master/README/UnotesScreenShot00002.png' alt='UNotes Browse Notes' width='50%'></img>

## Exporting your notes to a TXT file

You can export your notes to a text file.
Only UNotes that belong to the current scene can be exported.
To export, go to _Window > UNotes > Export to TXT file_.

## Collaborative Use

While UNotes is not optimized for collaboration and team work, the main reason being that the database file must be binary, a few features were put in place with this in mind. Skipping the explanations of the internals, the only part that should concern you is the database file that UNotes uses to store all notes; this, by default, is named _UNotesDatabase.dat_. You can decide to target a new file, for example _RicUNotesDatabase.dat_, by changing it in _Window > UNotes > Options > Show Advanced_. This allows all team members to have a personal _dat_ file that can be tracked and, potentially, loaded by other team members.

Just bear in mind that if two team members work on the same dat file it would be impossible to diff and do advanced merging due, again, to the binary nature of the database.

Finally, take extra care when changing the database filename as you might lose some unsaved notes.

# Contributions and Feedback

All feedback is welcome, be it pure love, suggestions or criticism.
Contributions are welcome, especially if you can take on one of the known issues. Please get in touch!
See contacts below.

# Contacts

email: unitysupport@riccardostecca.net

Follow me on twitter: @riccardostecca

# Desirable Improvements and Known Issues

* Feature: Undo / Redo is not yet implemented
* Issue: There's a glitch when selecting a note from the block which causes the UNote Editor to not update its content to the newly selected note. The GameObject though is correctly selected so, for now, you can work around this by re-selecting the GameObject with a click on it.
* Feature: Button in UNote Editor to create a note when the selected GameObject doesn't have one.
* Feature: Have an optional gizmo in Scene/Game View to see an object has a note attached.
* Feature: Set hierarchy dots to appear left or right or with an offset to avoid overlapping with other indicators rendered by other hierarchy plugins.
