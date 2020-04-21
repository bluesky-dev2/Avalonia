
#include "common.h"
#include "menu.h"
#include "window.h"

@implementation AvnMenu
{
    NSObject<NSMenuDelegate>* _wtf;
}
- (id) initWithDelegate: (NSObject<NSMenuDelegate>*)del
{
    self = [super init];
    self.delegate = del;
    _wtf = del;
    return self;
}

@end

@implementation AvnMenuItem
{
    AvnAppMenuItem* _item;
}

- (id) initWithAvnAppMenuItem: (AvnAppMenuItem*)menuItem
{
    if(self != nil)
    {
        _item = menuItem;
        self = [super initWithTitle:@""
                             action:@selector(didSelectItem:)
                      keyEquivalent:@""];
        
        [self setEnabled:YES];
        
        [self setTarget:self];
    }
    
    return self;
}

- (BOOL)validateMenuItem:(NSMenuItem *)menuItem
{
    if([self submenu] != nil)
    {
        return YES;
    }
    
    return _item->EvaluateItemEnabled();
}

- (void)didSelectItem:(nullable id)sender
{
    _item->RaiseOnClicked();
}
@end

AvnAppMenuItem::AvnAppMenuItem(bool isSeperator)
{
    _isSeperator = isSeperator;
    
    if(isSeperator)
    {
        _native = [NSMenuItem separatorItem];
    }
    else
    {
        _native = [[AvnMenuItem alloc] initWithAvnAppMenuItem: this];
    }
    
    _callback = nullptr;
}

NSMenuItem* AvnAppMenuItem::GetNative()
{
    return _native;
}

HRESULT AvnAppMenuItem::SetSubMenu (IAvnMenu* menu)
{
    if(menu != nullptr)
    {
        auto nsMenu = dynamic_cast<AvnAppMenu*>(menu)->GetNative();
        
        [_native setSubmenu: nsMenu];
    }
    else
    {
        [_native setSubmenu: nullptr];
    }
    
    return S_OK;
}

HRESULT AvnAppMenuItem::SetTitle (void* utf8String)
{
    if (utf8String != nullptr)
    {
        [_native setTitle:[NSString stringWithUTF8String:(const char*)utf8String]];
    }
    
    return S_OK;
}

HRESULT AvnAppMenuItem::SetGesture (void* key, AvnInputModifiers modifiers)
{
    NSEventModifierFlags flags = 0;
    
    if (modifiers & Control)
        flags |= NSEventModifierFlagControl;
    if (modifiers & Shift)
        flags |= NSEventModifierFlagShift;
    if (modifiers & Alt)
        flags |= NSEventModifierFlagOption;
    if (modifiers & Windows)
        flags |= NSEventModifierFlagCommand;
    
    [_native setKeyEquivalent:[NSString stringWithUTF8String:(const char*)key]];
    [_native setKeyEquivalentModifierMask:flags];
    
    return S_OK;
}

HRESULT AvnAppMenuItem::SetAction (IAvnPredicateCallback* predicate, IAvnActionCallback* callback)
{
    _predicate = predicate;
    _callback = callback;
    return S_OK;
}

HRESULT AvnAppMenuItem::SetIsChecked (bool isChecked)
{
    [_native setState:(isChecked ? NSOnState : NSOffState)];
    return S_OK;
}

bool AvnAppMenuItem::EvaluateItemEnabled()
{
    if(_predicate != nullptr)
    {
        auto result = _predicate->Evaluate ();
        
        return result;
    }
    
    return false;
}

void AvnAppMenuItem::RaiseOnClicked()
{
    if(_callback != nullptr)
    {
        _callback->Run();
    }
}

AvnAppMenu::AvnAppMenu()
{
    id del = [[AvnMenuDelegate alloc] initWithParent: this];
    _native = [[AvnMenu alloc] initWithDelegate: del];
}


AvnMenu* AvnAppMenu::GetNative()
{
    return _native;
}

HRESULT AvnAppMenu::InsertItem(int index, IAvnMenuItem *item)
{
    auto avnMenuItem = dynamic_cast<AvnAppMenuItem*>(item);
    
    if(avnMenuItem != nullptr)
    {
        [_native insertItem: avnMenuItem->GetNative() atIndex:index];
    }
    
    return S_OK;
}

HRESULT AvnAppMenu::RemoveItem (IAvnMenuItem* item)
{
    auto avnMenuItem = dynamic_cast<AvnAppMenuItem*>(item);
    
    if(avnMenuItem != nullptr)
    {
        [_native removeItem:avnMenuItem->GetNative()];
    }
    
    return S_OK;
}

HRESULT AvnAppMenu::SetTitle (void* utf8String)
{
    if (utf8String != nullptr)
    {
        [_native setTitle:[NSString stringWithUTF8String:(const char*)utf8String]];
    }
    
    return S_OK;
}

HRESULT AvnAppMenu::Clear()
{
    [_native removeAllItems];
    return S_OK;
}

@implementation AvnMenuDelegate
{
    ComPtr<AvnAppMenu> _parent;
}
- (id) initWithParent:(AvnAppMenu *)parent
{
    self = [super init];
    _parent = parent;
    return self;
}
- (BOOL)menu:(NSMenu *)menu updateItem:(NSMenuItem *)item atIndex:(NSInteger)index shouldCancel:(BOOL)shouldCancel
{
    if(shouldCancel)
        return NO;
    return YES;
}

- (NSInteger)numberOfItemsInMenu:(NSMenu *)menu
{
    return [menu numberOfItems];
}

- (void)menuNeedsUpdate:(NSMenu *)menu
{
    printf("NEEDSUPDATE\n");
}


@end

extern IAvnMenu* CreateAppMenu(IAvnMenuEvents* cb)
{
    @autoreleasepool
    {
        return new AvnAppMenu();
    }
}

extern IAvnMenuItem* CreateAppMenuItem()
{
    @autoreleasepool
    {
        return new AvnAppMenuItem(false);
    }
}

extern IAvnMenuItem* CreateAppMenuItemSeperator()
{
    @autoreleasepool
    {
        return new AvnAppMenuItem(true);
    }
}

static IAvnMenu* s_appMenu = nullptr;
static NSMenuItem* s_appMenuItem = nullptr;

extern void SetAppMenu (NSString* appName, IAvnMenu* menu)
{
    s_appMenu = menu;
    
    if(s_appMenu != nullptr)
    {
        auto nativeMenu = dynamic_cast<AvnAppMenu*>(s_appMenu);
        
        auto currentMenu = [s_appMenuItem menu];
        
        if (currentMenu != nullptr)
        {
            [currentMenu removeItem:s_appMenuItem];
        }
        
        s_appMenuItem = [nativeMenu->GetNative() itemAtIndex:0];
        
        if (currentMenu == nullptr)
        {
            currentMenu = [s_appMenuItem menu];
        }
        
        [[s_appMenuItem menu] removeItem:s_appMenuItem];
        
        [currentMenu insertItem:s_appMenuItem atIndex:0];
        
        if([s_appMenuItem submenu] == nullptr)
        {
            [s_appMenuItem setSubmenu:[NSMenu new]];
        }
        
        auto appMenu  = [s_appMenuItem submenu];
        
        [appMenu addItem:[NSMenuItem separatorItem]];
        
        // Services item and menu
        auto servicesItem = [[NSMenuItem alloc] init];
        servicesItem.title = @"Services";
        NSMenu *servicesMenu = [[NSMenu alloc] initWithTitle:@"Services"];
        servicesItem.submenu = servicesMenu;
        [NSApplication sharedApplication].servicesMenu = servicesMenu;
        [appMenu addItem:servicesItem];
        
        [appMenu addItem:[NSMenuItem separatorItem]];
        
        // Hide Application
        auto hideItem = [[NSMenuItem alloc] initWithTitle:[@"Hide " stringByAppendingString:appName] action:@selector(hide:) keyEquivalent:@"h"];
        
        [appMenu addItem:hideItem];
        
        // Hide Others
        auto hideAllOthersItem = [[NSMenuItem alloc] initWithTitle:@"Hide Others"
                                                       action:@selector(hideOtherApplications:)
                                                keyEquivalent:@"h"];
        
        hideAllOthersItem.keyEquivalentModifierMask = NSEventModifierFlagCommand | NSEventModifierFlagOption;
        [appMenu addItem:hideAllOthersItem];
        
        // Show All
        auto showAllItem = [[NSMenuItem alloc] initWithTitle:@"Show All"
                                                 action:@selector(unhideAllApplications:)
                                          keyEquivalent:@""];
        
        [appMenu addItem:showAllItem];
        
        [appMenu addItem:[NSMenuItem separatorItem]];
        
        // Quit Application
        auto quitItem = [[NSMenuItem alloc] init];
        quitItem.title = [@"Quit " stringByAppendingString:appName];
        quitItem.keyEquivalent = @"q";
        quitItem.target = [AvnWindow class];
        quitItem.action = @selector(closeAll);
        [appMenu addItem:quitItem];
    }
    else
    {
        s_appMenuItem = nullptr;
    }
}

extern IAvnMenu* GetAppMenu ()
{
    return s_appMenu;
}

extern NSMenuItem* GetAppMenuItem ()
{
    return s_appMenuItem;
}


