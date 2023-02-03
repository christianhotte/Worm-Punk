/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Facebook.WitAi.Events.Editor;
using UnityEditor;

namespace Facebook.WitAi.Dictation.Events.Editor
{
    [CustomPropertyDrawer(typeof(DictationEvents))]
    public class DictationEventPropertyDrawer : EventPropertyDrawer<DictationEvents>
    {
    }
}
